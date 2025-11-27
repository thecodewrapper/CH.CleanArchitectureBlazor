using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure.Plugins
{
    internal class LoggingPlugin : IServiceBusReceiverPlugin, IServiceBusSenderPlugin
    {
        private readonly ILogger<LoggingPlugin> _logger;

        public LoggingPlugin(ILogger<LoggingPlugin> logger) {
            _logger = logger;
        }

        public Task AfterMessageReceive(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default) {
            _logger.LogInformation("Received message with ID: {MessageId}, Correlation ID: {CorrelationId}, Subject: {Subject}", 
                message.MessageId, 
                message.CorrelationId, 
                message.Subject);

            return Task.CompletedTask;
        }

        public Task BeforeMessageSend(ServiceBusMessage message, CancellationToken cancellationToken = default) {
            _logger.LogInformation("Sending message with ID: {MessageId}, Correlation ID: {CorrelationId}, Subject: {Subject}", 
                message.MessageId, 
                message.CorrelationId, 
                message.Subject);

            return Task.CompletedTask;
        }
    }
}
