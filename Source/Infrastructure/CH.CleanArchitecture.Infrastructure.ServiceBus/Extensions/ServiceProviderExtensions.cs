using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.ServiceBus.Azure;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public static class ServiceProviderExtensions
    {
        public static void UseServiceBus(this IServiceProvider serviceProvider) {
            var options = serviceProvider.GetRequiredService<IOptions<ServiceBusOptions>>();
            if (!options.Value.Enabled) {
                return;
            }

            UseServiceBusCore(serviceProvider);

            // Ensure reply queue exists
            var replyQueueResolver = serviceProvider.GetRequiredService<ReplyQueueResolver>();
            var serviceBusManager = serviceProvider.GetRequiredService<IServiceBusManager>();
            serviceBusManager.EnsureQueueExistsAsync(replyQueueResolver.GetReplyQueueName()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Bootstrap the service bus core services, regardless of message broker provider
        /// </summary>
        /// <param name="services"></param>
        private static void UseServiceBusCore(IServiceProvider services) {
            var manager = services.GetRequiredService<IServiceBusManager>();
            var registry = services.GetRequiredService<IMessageRegistry<IRequest>>();

            // Automatically subscribe to all registered consumers
            foreach (var messageType in registry.GetConsumableTypes()) {
                typeof(IServiceBusManager)
                    .GetMethod(nameof(IServiceBusManager.SubscribeToMessage))!
                    .MakeGenericMethod(messageType)
                    .Invoke(manager, null);
            }
        }
    }
}
