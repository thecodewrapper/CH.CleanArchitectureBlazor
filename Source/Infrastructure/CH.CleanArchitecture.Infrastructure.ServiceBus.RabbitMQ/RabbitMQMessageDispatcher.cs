using CH.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions;
using RabbitMQ.Client;
using System.Text;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ
{
    internal class RabbitMQMessageDispatcher : IMessageBrokerDispatcher
    {
        private readonly ILogger<RabbitMQMessageDispatcher> _logger;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageResponseTracker _tracker;
        private readonly ITopicNameFormatter _topicNameFormatter;
        private readonly string _instanceId;
        private readonly string _replyQueueName;

        private readonly IConnection _connection;

        public RabbitMQMessageDispatcher(
            ILogger<RabbitMQMessageDispatcher> logger,
            RabbitMQConnectionManager connectionManager,
            IMessageSerializer serializer,
            IMessageResponseTracker tracker,
            IServiceBusNaming naming,
            ITopicNameFormatter topicNameFormatter) {
            _logger = logger;
            _connection = connectionManager.GetOrCreateConnectionAsync().GetAwaiter().GetResult();
            _serializer = serializer;
            _tracker = tracker;
            _topicNameFormatter = topicNameFormatter;
            _instanceId = naming.GetInstanceId().ToString(); // Use same naming class for consistency
            _replyQueueName = naming.GetReplyQueueName(); // Name used by RabbitMqResponseListener
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            where TResponse : class {
            var topicName = _topicNameFormatter.GetTopicName(request.GetType());
            var baseMessage = request as BaseMessage;
            Guid correlationId = baseMessage?.CorrelationId ?? Guid.NewGuid();
            string? recipient = baseMessage?.Recipient;


            using var channel = await _connection.CreateChannelAsync();
            BasicProperties props = new()
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyQueueName,
                Type = topicName,
                Headers = new Dictionary<string, object>
                {
                    ["InstanceId"] = Encoding.UTF8.GetBytes(_instanceId)
                }
            };

            string bodyStr = _serializer.Serialize(request);
            byte[] body = Encoding.UTF8.GetBytes(bodyStr);
            await channel.BasicPublishAsync(
                exchange: topicName,
                routingKey: "",
                mandatory: false,
                basicProperties: props,
                body: body
            );

            _logger.LogDebug("Sent message {Type} to exchange {Exchange} with CorrelationId {CorrelationId}. Body: {Body}", typeof(TResponse).Name, topicName, correlationId, bodyStr);

            return await _tracker.WaitForResponseAsync<TResponse>(correlationId, TimeSpan.FromSeconds(10));
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class, IRequest {
            var topicName = _topicNameFormatter.GetTopicName(@event.GetType());
            string bodyStr = _serializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(bodyStr);

            using var channel = await _connection.CreateChannelAsync();
            BasicProperties props = new()
            {
                Type = topicName,
                Headers = new Dictionary<string, object>
                {
                    ["InstanceId"] = Encoding.UTF8.GetBytes(_instanceId)
                }
            };

            await channel.BasicPublishAsync(
                exchange: topicName,
                routingKey: "",
                mandatory: false,
                basicProperties: props,
                body: body
            );

            _logger.LogDebug("Published event {Type} to exchange {Exchange}. Body: {Body}", @event.GetType().Name, topicName, bodyStr);
        }
    }
}
