using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ
{
    internal class RabbitMQResponseListener : BackgroundService
    {
        private readonly ILogger<RabbitMQResponseListener> _logger;
        private readonly RabbitMQConnectionManager _connectionManager;
        private readonly IMessageResponseTracker _tracker;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageBrokerManager _manager;
        private readonly string _replyQueueName;

        public RabbitMQResponseListener(
            ILogger<RabbitMQResponseListener> logger,
            RabbitMQConnectionManager connectionManager,
            IMessageResponseTracker tracker,
            IMessageSerializer serializer,
            IMessageBrokerManager manager,
            ServiceBusNaming naming) {
            _logger = logger;
            _connectionManager = connectionManager;
            _tracker = tracker;
            _serializer = serializer;
            _manager = manager;
            _replyQueueName = naming.GetReplyQueueName();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _logger.LogInformation("Starting RabbitMQ response listener on queue '{Queue}'", _replyQueueName);

            var connection = await _connectionManager.CreateConnectionAsync();
            await _manager.CreateQueueAsync(_replyQueueName);

            var channel = await connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, args) =>
            {
                try {
                    var correlationId = args.BasicProperties?.CorrelationId;
                    var subject = args.BasicProperties?.Type;

                    if (string.IsNullOrEmpty(correlationId) || string.IsNullOrEmpty(subject)) {
                        _logger.LogWarning("Missing CorrelationId or Type on reply message.");
                        return;
                    }

                    var responseType = Type.GetType(subject);
                    if (responseType == null) {
                        _logger.LogWarning("Could not resolve response type: {Type}", subject);
                        return;
                    }

                    var body = Encoding.UTF8.GetString(args.Body.ToArray());
                    var deserialized = _serializer.Deserialize(body, responseType);

                    _logger.LogDebug("Received response for CorrelationId: {CorrelationId}", correlationId);

                    typeof(IMessageResponseTracker)
                        .GetMethod(nameof(IMessageResponseTracker.SetResponse))!
                        .MakeGenericMethod(responseType)
                        .Invoke(_tracker, new[] { Guid.Parse(correlationId), deserialized });
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Error processing reply message.");
                }

                await Task.Yield(); // async event handler contract
            };

            await channel.BasicConsumeAsync(_replyQueueName, autoAck: true, consumer);

            _logger.LogInformation("RabbitMQ response listener initialized.");
        }
    }
}