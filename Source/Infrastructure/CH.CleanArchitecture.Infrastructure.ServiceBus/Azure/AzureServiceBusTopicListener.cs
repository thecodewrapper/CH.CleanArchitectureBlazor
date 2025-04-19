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
            foreach (var messageType in _registry.GetConsumableTypes()) {
                var topicName = TopicNameHelper.GetTopicName(messageType);

                //Ensure topic and subscription exist
                await EnsureTopicWithSubscriptionAsync(topicName, _subscriptionName);

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

        private async Task EnsureTopicWithSubscriptionAsync(string topicName, string subscriptionName) {
            //Ensure topic exists
            if (!await _adminClient.TopicExistsAsync(topicName))
                await _adminClient.CreateTopicAsync(topicName);

            //Ensure subscription exists
            if (!await _adminClient.SubscriptionExistsAsync(topicName, subscriptionName)) {
                await _adminClient.CreateSubscriptionAsync(topicName, subscriptionName);

                //Filter to only receive messages of this type
                var rule = new CreateRuleOptions
                {
                    Name = "MessageTypeFilter",
                    Filter = new SqlRuleFilter($"[Type] = '{topicName}'")
                };

                await _adminClient.DeleteRuleAsync(topicName, subscriptionName, "$Default");
                await _adminClient.CreateRuleAsync(topicName, subscriptionName, rule);
            }
        }

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

        private async Task SendReplyMessageToBus(ServiceBusReceivedMessage originalMessage, object response) {
            var replySender = _client.CreateSender(originalMessage.ReplyTo);
            string serializedResponse = _serializer.Serialize(response);
            var replyMessage = new ServiceBusMessage(serializedResponse)
            {
                CorrelationId = originalMessage.CorrelationId,
                Subject = response.GetType().AssemblyQualifiedName
            };

            await replySender.SendMessageAsync(replyMessage);
        }
    }
}