using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ
{
    internal class RabbitMQManager : IMessageBrokerManager
    {
        private readonly ILogger<RabbitMQManager> _logger;
        private readonly IConnection _connection;

        public RabbitMQManager(ILogger<RabbitMQManager> logger, RabbitMQConnectionManager connectionManager) {
            _logger = logger;
            _connection = connectionManager.CreateConnectionAsync().GetAwaiter().GetResult();
        }

        public async Task CreateQueueAsync(string queueName) {
            using var channel = await _connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            _logger.LogInformation("Queue declared: {QueueName}", queueName);
        }

        public async Task CreateTopicAsync(string topicName) {
            using var channel = await _connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchange: topicName, type: ExchangeType.Topic, durable: true);
            _logger.LogInformation("Exchange declared: {TopicName}", topicName);
        }

        public async Task CreateSubscriptionAsync(string topicName, string subscriptionName) {
            using var channel = await _connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(subscriptionName, durable: true, exclusive: false, autoDelete: false);
            await channel.ExchangeDeclareAsync(topicName, ExchangeType.Topic, durable: true);
            await channel.QueueBindAsync(queue: subscriptionName, exchange: topicName, routingKey: "#");
            _logger.LogInformation("Queue {SubscriptionName} bound to topic {TopicName}", subscriptionName, topicName);
        }
    }
}
