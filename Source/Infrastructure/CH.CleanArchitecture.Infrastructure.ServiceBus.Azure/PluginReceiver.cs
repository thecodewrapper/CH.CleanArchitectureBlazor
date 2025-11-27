using Azure.Messaging.ServiceBus;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    internal class PluginReceiver : ServiceBusReceiver
    {
        private IEnumerable<IServiceBusReceiverPlugin> _plugins;

        internal PluginReceiver(string queueName, ServiceBusClient client, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusReceiverOptions options) :
            base(client, queueName, options) {
            _plugins = plugins;
        }

        internal PluginReceiver(string topicName, string subscriptionName, ServiceBusClient client, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusReceiverOptions options) :
            base(client, topicName, subscriptionName, options) {
            _plugins = plugins;
        }

        public override async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(TimeSpan? maxWaitTime = null, CancellationToken cancellationToken = default) {
            ServiceBusReceivedMessage message = await base.ReceiveMessageAsync(maxWaitTime, cancellationToken).ConfigureAwait(false);

            foreach (var plugin in _plugins) {
                await plugin.AfterMessageReceive(message);
            }
            return message;
        }
    }
}
