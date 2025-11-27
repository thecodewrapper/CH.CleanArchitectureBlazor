using Azure.Messaging.ServiceBus;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    internal static class ServiceBusClientExtensions
    {
        public static PluginSender CreatePluginSender(this ServiceBusClient client, string queueOrTopicName, IEnumerable<IServiceBusSenderPlugin> plugins) {
            return new PluginSender(queueOrTopicName, client, plugins);
        }

        public static PluginReceiver CreatePluginReceiver(this ServiceBusClient client, string queueName, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusReceiverOptions options = default) {
            return new PluginReceiver(queueName, client, plugins, options ?? new ServiceBusReceiverOptions());
        }

        public static PluginReceiver CreatePluginReceiver(this ServiceBusClient client, string topicName, string subscriptionName, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusReceiverOptions options = default) {
            return new PluginReceiver(topicName, subscriptionName, client, plugins, options ?? new ServiceBusReceiverOptions());
        }

        public static PluginProcessor CreatePluginProcessor(this ServiceBusClient client, string queueName, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusProcessorOptions options = default) {
            return new PluginProcessor(queueName, client, plugins, options ?? new ServiceBusProcessorOptions());
        }

        public static PluginProcessor CreatePluginProcessor(this ServiceBusClient client, string topicName, string subscriptionName, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusProcessorOptions options = default) {
            return new PluginProcessor(topicName, subscriptionName, client, plugins, options ?? new ServiceBusProcessorOptions());
        }

        public static PluginSessionProcessor CreatePluginSessionProcessor(this ServiceBusClient client, string queueName, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusSessionProcessorOptions options = default) {
            return new PluginSessionProcessor(queueName, client, plugins, options ?? new ServiceBusSessionProcessorOptions());
        }

        public static PluginSessionProcessor CreatePluginSessionProcessor(this ServiceBusClient client, string topicName, string subscriptionName, IEnumerable<IServiceBusReceiverPlugin> plugins, ServiceBusSessionProcessorOptions options = default) {
            return new PluginSessionProcessor(topicName, subscriptionName, client, plugins, options ?? new ServiceBusSessionProcessorOptions());
        }
    }
}
