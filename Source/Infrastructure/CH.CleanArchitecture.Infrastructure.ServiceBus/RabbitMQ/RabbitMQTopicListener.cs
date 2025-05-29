using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ
{
    internal class RabbitMQTopicListener : BackgroundService
    {
        private readonly ILogger<RabbitMQTopicListener> _logger;
        private readonly IConnection _connection;
        private readonly IMessageBrokerManager _manager;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageRegistry<IRequest> _registry;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _subscriptionQueue;

        private readonly Dictionary<string, IChannel> _channels = new();

        public RabbitMQTopicListener(
            ILogger<RabbitMQTopicListener> logger,
            RabbitMQConnectionManager connectionManager,
            IMessageBrokerManager manager,
            IMessageSerializer serializer,
            IMessageRegistry<IRequest> registry,
            IServiceScopeFactory scopeFactory,
            ServiceBusNaming naming) {
            _logger = logger;
            _connection = connectionManager.GetOrCreateConnectionAsync().GetAwaiter().GetResult();
            _manager = manager;
            _serializer = serializer;
            _registry = registry;
            _scopeFactory = scopeFactory;
            _subscriptionQueue = naming.GetSubscriptionName();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Starting RabbitMQ topic listener with subscription queue: {Queue}", _subscriptionQueue);

            var consumableTypes = _registry.GetConsumableTypes();
            var producableTypes = _registry.GetProducableTypes();

            var consumableTopics = consumableTypes.Select(TopicNameHelper.GetTopicName).ToList();
            var producableTopics = producableTypes.Select(TopicNameHelper.GetTopicName).ToList();

            await EnsureTopicsExist(producableTopics.Concat(consumableTopics).Distinct());
            await EnsureSubscriptionsExist(consumableTopics);

            foreach (var messageType in consumableTypes) {
                var topicName = TopicNameHelper.GetTopicName(messageType);
                await StartProcessingTopicAsync(topicName, messageType, cancellationToken);
            }

            _logger.LogInformation("RabbitMQ topic listener started successfully.");
        }

        public override async Task StopAsync(CancellationToken cancellationToken) {
            foreach (var channel in _channels.Values) {
                await channel.CloseAsync();
                await channel.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }

        private async Task EnsureTopicsExist(IEnumerable<string> topicNames) {
            foreach (var topic in topicNames) {
                await _manager.CreateTopicAsync(topic);
            }
        }

        private async Task EnsureSubscriptionsExist(IEnumerable<string> topicNames) {
            foreach (var topic in topicNames) {
                await _manager.CreateSubscriptionAsync(topic, _subscriptionQueue);
            }
        }

        private async Task StartProcessingTopicAsync(string topicName, Type messageType, CancellationToken cancellationToken) {
            var channel = await _connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, args) =>
            {
                try {
                    string raw = Encoding.UTF8.GetString(args.Body.ToArray());
                    string? subject = args.BasicProperties?.Type;

                    if (string.IsNullOrWhiteSpace(subject)) {
                        _logger.LogWarning("Missing Type property on incoming message.");
                        return;
                    }

                    var payload = _serializer.Deserialize(raw, messageType);

                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IServiceBusMediator>();

                    if (payload is BaseMessage baseMessage) {
                        if (baseMessage.IsEvent) {
                            await mediator.PublishAsync((IRequest)payload, cancellationToken);
                        }
                        else {
                            var response = await SendMessageToMediatorAsync(baseMessage, payload, mediator, cancellationToken);

                            if (!string.IsNullOrWhiteSpace(args.BasicProperties?.ReplyTo)) {
                                await SendReplyAsync(channel, args, response);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Error processing message from {Topic}", topicName);
                }

                await Task.Yield(); // for async consumer contract
            };

            await channel.BasicConsumeAsync(_subscriptionQueue, autoAck: true, consumer);
            _channels[topicName] = channel;

            _logger.LogInformation("Started consuming topic: {Topic} (type: {Type})", topicName, messageType.Name);
        }

        private async Task<object?> SendMessageToMediatorAsync(BaseMessage baseMessage, object payload, IServiceBusMediator mediator, CancellationToken ct) {
            var responseType = Type.GetType(baseMessage.ResponseType);
            if (responseType == null)
                throw new InvalidOperationException($"Could not resolve response type: {baseMessage.ResponseType}");

            var method = typeof(IServiceBus)
                .GetMethod(nameof(IServiceBus.SendAsync))!
                .MakeGenericMethod(responseType);

            var task = (Task)method.Invoke(mediator, new[] { payload, ct })!;
            await task.ConfigureAwait(false);

            return task.GetType().GetProperty("Result")?.GetValue(task);
        }

        private async Task SendReplyAsync(IChannel channel, BasicDeliverEventArgs originalArgs, object response) {
            var replyTo = originalArgs.BasicProperties.ReplyTo;
            var correlationId = originalArgs.BasicProperties.CorrelationId;

            string responsePayload = _serializer.Serialize(response);
            var props = new BasicProperties
            {
                CorrelationId = correlationId,
                Type = response.GetType().AssemblyQualifiedName
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: replyTo,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(responsePayload)
            );

            _logger.LogInformation("Sent reply to {ReplyTo} with CorrelationId {CorrelationId}", replyTo, correlationId);
        }
    }
}