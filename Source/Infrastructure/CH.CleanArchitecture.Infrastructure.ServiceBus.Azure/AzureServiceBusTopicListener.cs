using Azure.Messaging.ServiceBus;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    /// <summary>
    /// Background service that listens to all Azure Service Bus topics for subscribed message types.
    /// Each topic corresponds to one message type. Messages are routed to internal mediator via IServiceBusMediator.
    /// </summary>
    internal class AzureServiceBusTopicListener : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ITopicNameFormatter _topicNameFormatter;
        private readonly IMessageRegistry<IRequest> _registry;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageBrokerManager _manager;
        private readonly ILogger<AzureServiceBusTopicListener> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _subscriptionName;
        private readonly Dictionary<string, ServiceBusProcessor> _processors = new();

        public AzureServiceBusTopicListener(ILogger<AzureServiceBusTopicListener> logger,
            IMessageRegistry<IRequest> registry,
            IMessageSerializer serializer,
            IMessageBrokerManager manager,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ServiceBusClient serviceBusClient,
            IServiceBusNaming serviceBusNaming,
            ITopicNameFormatter topicNameFormatter) {

            _registry = registry;
            _serializer = serializer;
            _manager = manager;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            _client = serviceBusClient;
            _topicNameFormatter = topicNameFormatter;
            _subscriptionName = serviceBusNaming.GetSubscriptionName();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
            _logger.LogInformation($"Starting Azure Service Bus topic listener with subscription name: {_subscriptionName}");

            IEnumerable<Type> consumableMessageTypes = _registry.GetConsumableTypes();

            List<string> consumableTopicNames = consumableMessageTypes.Select(_topicNameFormatter.GetTopicName).ToList();
            List<string> producableTopicNames = _registry.GetProducableTypes().Select(_topicNameFormatter.GetTopicName).ToList();

            await EnsureTopicsExist(producableTopicNames);
            await EnsureTopicsExist(consumableTopicNames);
            await EnsureSubscriptionsExist(consumableTopicNames);

            //Start listening to consumable types
            foreach (var messageType in consumableMessageTypes) {
                var topicName = _topicNameFormatter.GetTopicName(messageType);

                try {
                    await StartProcessingTopicAsync(topicName, messageType, cancellationToken);
                }
                catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound) {
                    _logger.LogWarning(ex, "Subscription or topic not found for {Topic}. Recreating and retrying...", topicName);

                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); //give Azure time to catch-up

                    await _manager.CreateTopicAsync(topicName);
                    await _manager.CreateSubscriptionAsync(topicName, _subscriptionName);
                    await StartProcessingTopicAsync(topicName, messageType, cancellationToken);
                }
            }

            _logger.LogInformation("Azure Service Bus topic listener started successfully.");
        }

        public override async Task StopAsync(CancellationToken cancellationToken) {
            foreach (var processor in _processors.Values) {
                await processor.StopProcessingAsync(cancellationToken);
                await processor.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }

        private async Task EnsureTopicsExist(List<string> topicNames) {
            foreach (var topicName in topicNames) {
                await _manager.CreateTopicAsync(topicName);
            }
        }

        private async Task EnsureSubscriptionsExist(List<string> topicNames) {
            foreach (var topicName in topicNames) {
                await _manager.CreateSubscriptionAsync(topicName, _subscriptionName);
            }
        }

        private async Task StartProcessingTopicAsync(string topicName, Type messageType, CancellationToken cancellationToken) {
            var processor = _client.CreateProcessor(topicName, _subscriptionName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });

            processor.ProcessMessageAsync += async args => await HandleMessageAsync(args, messageType);
            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Error in processor for {Topic}", topicName);
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(cancellationToken);
            _processors[topicName] = processor;

            _logger.LogInformation("Started listening to topic: {Topic} (type: {Type})", topicName, messageType.Name);
        }

        /// <summary>
        /// Handles an incoming Azure Service Bus message by deserializing it, resolving the appropriate mediator,
        /// and routing the message to either a publish or send handler, depending on whether it is an event or a command.
        /// If the message expects a response (command with ReplyTo set), the response is serialized and sent back
        /// to the specified reply queue. Completes or abandons the message based on success or failure.
        /// </summary>
        /// <param name="args">The message event arguments containing the received message and cancellation token.</param>
        /// <param name="messageType">The CLR type of the message payload.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task HandleMessageAsync(ProcessMessageEventArgs args, Type messageType) {
            try {
                string raw = args.Message.Body.ToString();
                var payload = _serializer.Deserialize(raw, messageType);

                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IServiceBusMediator>();

                if (payload is BaseMessage baseMessage) {
                    if (baseMessage.IsEvent) {

                        await mediator.PublishAsync((IRequest)payload, args.CancellationToken);
                    }
                    else {
                        var response = await SendMessageToMediatorAsync(baseMessage, payload, mediator, args.CancellationToken);

                        if (!string.IsNullOrEmpty(args.Message.ReplyTo) && response != null) {
                            await SendReplyMessageToBus(args.Message, response);
                        }
                    }
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to handle message of type {Type}", messageType.Name);
                await args.AbandonMessageAsync(args.Message);
            }
        }

        /// <summary>
        /// Invokes the <see cref="IServiceBusMediator"/> to send the message, gets the response, and returns the response object.
        /// </summary>
        /// <param name="baseMessage"></param>
        /// <param name="payload"></param>
        /// <param name="mediator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<object?> SendMessageToMediatorAsync(BaseMessage baseMessage, object payload, IServiceBusMediator mediator, CancellationToken cancellationToken) {
            var responseType = Type.GetType(baseMessage.ResponseType);
            if (responseType == null) {
                throw new InvalidOperationException($"Could not resolve response type: {baseMessage.ResponseType}");
            }

            var sendMethod = typeof(IServiceBus)
                .GetMethod(nameof(IServiceBus.SendAsync))!
                .MakeGenericMethod(responseType);

            var task = (Task)sendMethod.Invoke(mediator, new[] { payload, cancellationToken })!;
            await task.ConfigureAwait(false);

            var resultProp = task.GetType().GetProperty("Result");
            return resultProp?.GetValue(task);
        }

        /// <summary>
        /// Sends a reply message to the reply queue of the original sender of the message.
        /// </summary>
        /// <param name="originalMessage"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task SendReplyMessageToBus(ServiceBusReceivedMessage originalMessage, object response) {
            var replySender = _client.CreateSender(originalMessage.ReplyTo);
            string serializedResponse = _serializer.Serialize(response);
            var replyMessage = new ServiceBusMessage(serializedResponse)
            {
                CorrelationId = originalMessage.CorrelationId,
                Subject = response.GetType().AssemblyQualifiedName
            };

            _logger.LogInformation("Sending reply message to {ReplyTo} with correlation id {CorrelationId}", originalMessage.ReplyTo, originalMessage.CorrelationId);
            await replySender.SendMessageAsync(replyMessage);
        }
    }
}