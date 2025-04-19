using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CH.CleanArchitecture.Core.Application;
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
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly IMessageRegistry<IRequest> _registry;
        private readonly IMessageSerializer _serializer;
        private readonly ILogger<AzureServiceBusTopicListener> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _subscriptionName;
        private readonly Dictionary<string, ServiceBusProcessor> _processors = new();

        private HashSet<string> _existingTopics = new();
        private HashSet<string> _existingSubscriptions = new();

        public AzureServiceBusTopicListener(ILogger<AzureServiceBusTopicListener> logger,
            IMessageRegistry<IRequest> registry,
            IMessageSerializer serializer,
            ServiceBusClient serviceBusClient,
            ServiceBusAdministrationClient serviceBusAdministrationClient,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ServiceBusNaming serviceBusNaming) {

            _registry = registry;
            _serializer = serializer;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            _client = serviceBusClient;
            _adminClient = serviceBusAdministrationClient;
            _subscriptionName = serviceBusNaming.GetSubscriptionName();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _logger.LogInformation($"Starting Azure Service Bus topic listener with subscription name: {_subscriptionName}");
            await EnsureTopicsWithSubscriptionsAsync();

            foreach (var messageType in _registry.GetConsumableTypes()) {
                var topicName = TopicNameHelper.GetTopicName(messageType);

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

                await processor.StartProcessingAsync(stoppingToken);
                _processors[topicName] = processor;

                _logger.LogInformation("Started listening to topic: {Topic} (type: {Type})", topicName, messageType.Name);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken) {
            foreach (var processor in _processors.Values) {
                await processor.StopProcessingAsync(cancellationToken);
                await processor.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Ensures that all topics and subscriptions exist on Service Bus for the registered message types.
        /// </summary>
        /// <returns></returns>
        private async Task EnsureTopicsWithSubscriptionsAsync() {
            List<string> topicNamesFromTypes = _registry.GetConsumableTypes().Select(t => TopicNameHelper.GetTopicName(t).ToLowerInvariant()).ToList();

            _logger.LogInformation("Ensuring topics and subscriptions exist for message types: {Topics}", string.Join(", ", topicNamesFromTypes));
            var existingTopics = new HashSet<string>();
            await foreach (var topic in _adminClient.GetTopicsAsync()) {
                existingTopics.Add(topic.Name);
            }

            foreach (var topicName in topicNamesFromTypes) {
                // Create topic if missing
                if (!existingTopics.Contains(topicName)) {
                    _logger.LogInformation("Creating topic: {Topic}", topicName);
                    await _adminClient.CreateTopicAsync(topicName);
                    _logger.LogInformation("Created topic: {Topic}", topicName);
                }

                //Ensure subscription exists
                if (!await _adminClient.SubscriptionExistsAsync(topicName, _subscriptionName)) {
                    _logger.LogInformation("Creating subscription: {Subscription} for topic: {Topic}", _subscriptionName, topicName);
                    await _adminClient.CreateSubscriptionAsync(topicName, _subscriptionName);
                    _logger.LogInformation("Created subscription: {Subscription} for topic: {Topic}", _subscriptionName, topicName);

                    var rule = new CreateRuleOptions
                    {
                        Name = "MessageTypeFilter",
                        Filter = new SqlRuleFilter($"[Type] = '{topicName}' AND ([Recipient] IS NULL OR [Recipient] = '{_subscriptionName}')")
                    };

                    await _adminClient.DeleteRuleAsync(topicName, _subscriptionName, "$Default");
                    await _adminClient.CreateRuleAsync(topicName, _subscriptionName, rule);
                }
            }
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