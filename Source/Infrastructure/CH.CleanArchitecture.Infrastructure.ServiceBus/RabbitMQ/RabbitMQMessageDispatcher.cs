using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ
{
    internal class RabbitMQMessageDispatcher : IMessageBrokerDispatcher
    {
        private readonly ILogger<RabbitMQMessageDispatcher> _logger;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageResponseTracker _tracker;
        private readonly string _instanceId;
        private readonly string _replyQueueName;

        private readonly IConnection _connection;

        public RabbitMQMessageDispatcher(
            ILogger<RabbitMQMessageDispatcher> logger,
            RabbitMQConnectionManager connectionManager,
            IMessageSerializer serializer,
            IMessageResponseTracker tracker,
            ServiceBusNaming naming) {
            _logger = logger;
            _connection = connectionManager.CreateConnectionAsync().GetAwaiter().GetResult();
            _serializer = serializer;
            _tracker = tracker;
            _instanceId = naming.GetInstanceId().ToString(); // Use same naming class for consistency
            _replyQueueName = naming.GetReplyQueueName(); // Name used by RabbitMqResponseListener
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            where TResponse : class {
            var topicName = TopicNameHelper.GetTopicName(request.GetType());
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

            byte[] body = Encoding.UTF8.GetBytes(_serializer.Serialize(request));
            await channel.BasicPublishAsync(
                exchange: topicName,
                routingKey: "",
                mandatory: false,
                basicProperties: props,
                body: body
            );

            _logger.LogDebug("Sent message {Type} to exchange {Exchange} with CorrelationId {CorrelationId}", typeof(TResponse).Name, topicName, correlationId);

            return await _tracker.WaitForResponseAsync<TResponse>(correlationId, TimeSpan.FromSeconds(10));
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class, IRequest {
            var topicName = TopicNameHelper.GetTopicName(@event.GetType());
            var body = Encoding.UTF8.GetBytes(_serializer.Serialize(@event));

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

            _logger.LogDebug("Published event {Type} to exchange {Exchange}", typeof(TEvent).Name, topicName);
        }
    }
}
