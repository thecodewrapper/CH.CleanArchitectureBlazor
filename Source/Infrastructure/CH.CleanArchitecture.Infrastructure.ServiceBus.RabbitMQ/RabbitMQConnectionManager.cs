using CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ
{
    internal class RabbitMQConnectionManager
    {
        private readonly ILogger<RabbitMQConnectionManager> _logger;
        private readonly IServiceBusNaming _serviceBusNaming;
        private readonly ConnectionFactory _factory;

        private IConnection? _connection;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);

        public RabbitMQConnectionManager(string hostUrl, ILogger<RabbitMQConnectionManager> logger, IServiceBusNaming serviceBusNaming) {
            _logger = logger;
            _serviceBusNaming = serviceBusNaming;
            _factory = new ConnectionFactory
            {
                Uri = new Uri(hostUrl)
            };
        }

        public async Task<IConnection> GetOrCreateConnectionAsync() {
            if (_connection?.IsOpen == true)
                return _connection;

            await _connectionLock.WaitAsync();
            try {
                if (_connection?.IsOpen == true)
                    return _connection;

                _logger.LogInformation("Creating new RabbitMQ connection to {Uri}", _factory.Uri);
                _connection = await _factory.CreateConnectionAsync(_serviceBusNaming.GetInstanceId().ToString());

                return _connection;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to create RabbitMQ connection to {Uri}", _factory.Uri);
                throw;
            }
            finally {
                _connectionLock.Release();
            }
        }

        public bool IsConnected => _connection?.IsOpen == true;
    }
}