using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ
{
    public class RabbitMQConnectionManager
    {
        private readonly ILogger<RabbitMQConnectionManager> _logger;
        private readonly ConnectionFactory _factory;

        public RabbitMQConnectionManager(string hostUrl, ILogger<RabbitMQConnectionManager> logger) {
            _logger = logger;
            _factory = new ConnectionFactory
            {
                Uri = new Uri(hostUrl)
            };
        }

        public async Task<IConnection> CreateConnectionAsync() {
            _logger.LogInformation("Creating new RabbitMQ connection to {Uri}", _factory.Uri);
            return await _factory.CreateConnectionAsync();
        }
    }
}