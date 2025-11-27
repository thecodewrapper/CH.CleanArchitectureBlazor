using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureServiceBusMessaging(this IServiceCollection services, string hostUrl, IEnumerable<Type> producers, IEnumerable<Type> consumers) {
            // Azure Service Bus components
            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(hostUrl);
                builder.AddServiceBusAdministrationClient(hostUrl);
            });
            services.AddSingleton<IMessageBrokerManager, AzureServiceBusManager>();
            services.AddSingleton<IMessageBrokerDispatcher, AzureServiceBusMessageDispatcher>();

            services.AddHostedService<AzureServiceBusTopicListener>();
            services.AddHostedService<AzureServiceBusResponseListener>();

            return services;
        }

        public static IServiceCollection RegisterAzureServiceBusReceiverPlugin(this IServiceCollection services, IServiceBusReceiverPlugin serviceBusReceiverPlugin) {
            services.AddSingleton(serviceBusReceiverPlugin);
            return services;
        }

        public static IServiceCollection RegisterAzureServiceBusSenderPlugin(this IServiceCollection services, IServiceBusSenderPlugin serviceBusSenderPlugin) {
            services.AddSingleton(serviceBusSenderPlugin);
            return services;
        }

        public static IServiceCollection RegisterAzureServiceBusSenderPlugin<T>(this IServiceCollection services) where T : class, IServiceBusSenderPlugin {
            services.AddSingleton<IServiceBusSenderPlugin, T>();
            return services;
        }

        public static IServiceCollection RegisterAzureServiceBusReceiverPlugin<T>(this IServiceCollection services) where T : class, IServiceBusReceiverPlugin {
            services.AddSingleton<IServiceBusReceiverPlugin, T>();
            return services;
        }
    }
}
