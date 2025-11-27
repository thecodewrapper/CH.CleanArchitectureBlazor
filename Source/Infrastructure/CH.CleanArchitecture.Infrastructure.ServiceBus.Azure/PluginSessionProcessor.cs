using Azure.Messaging.ServiceBus;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    public class PluginSessionProcessor : ServiceBusSessionProcessor
    {
        private IEnumerable<IServiceBusReceiverPlugin> _plugins;

        internal PluginSessionProcessor(string queueName, ServiceBusClient client, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusSessionProcessorOptions options) :
            base(client, queueName, options) {
            _plugins = plugins;
        }

        internal PluginSessionProcessor(string topicName, string subscriptionName, ServiceBusClient client, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusSessionProcessorOptions options) :
            base(client, topicName, subscriptionName, options) {
            _plugins = plugins;
        }

        protected override async Task OnProcessSessionMessageAsync(ProcessSessionMessageEventArgs args) {
            foreach (var plugin in _plugins) {
                await plugin.AfterMessageReceive(args.Message);
            }

            await base.OnProcessSessionMessageAsync(args);
        }

        protected override Task OnProcessErrorAsync(ProcessErrorEventArgs args) {
            return Task.CompletedTask;
        }
    }
}
