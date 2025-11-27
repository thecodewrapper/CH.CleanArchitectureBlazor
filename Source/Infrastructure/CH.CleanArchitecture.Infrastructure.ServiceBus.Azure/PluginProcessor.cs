using Azure.Messaging.ServiceBus;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    public class PluginProcessor : ServiceBusProcessor
    {
        private IEnumerable<IServiceBusReceiverPlugin> _plugins;

        internal PluginProcessor(string queueName, ServiceBusClient client, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusProcessorOptions options) :
            base(client, queueName, options) {
            _plugins = plugins;
        }

        internal PluginProcessor(string topicName, string subscriptionName, ServiceBusClient client, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusProcessorOptions options) :
            base(client, topicName, subscriptionName, options) {
            _plugins = plugins;
        }

        protected override async Task OnProcessMessageAsync(ProcessMessageEventArgs args) {
            foreach (var plugin in _plugins) {
                await plugin.AfterMessageReceive(args.Message);
            }

            await base.OnProcessMessageAsync(args);
        }
    }
}
