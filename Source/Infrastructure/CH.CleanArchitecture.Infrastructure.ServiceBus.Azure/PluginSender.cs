using Azure.Messaging.ServiceBus;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    internal class PluginSender : ServiceBusSender
    {
        private IEnumerable<IServiceBusSenderPlugin> _plugins;

        internal PluginSender(string queueOrTopicName, ServiceBusClient client, IEnumerable<IServiceBusSenderPlugin> plugins) : base(client, queueOrTopicName) {
            _plugins = plugins;
        }

        public override async Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default) {
            foreach (var plugin in _plugins) {
                await plugin.BeforeMessageSend(message, cancellationToken);
            }
            await base.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
