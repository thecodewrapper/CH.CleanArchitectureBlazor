using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.ServiceBus.Azure;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureServiceBusMessaging(this IServiceCollection services, string hostUrl, IEnumerable<Type> producers, IEnumerable<Type> consumers) {
            services.AddServiceBusCore(producers, consumers);

            // Azure Service Bus components
            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(hostUrl);
                builder.AddServiceBusAdministrationClient(hostUrl);
            });
            services.AddSingleton<IMessageBrokerDispatcher, AzureServiceBusMessageDispatcher>();
            services.AddSingleton<IServiceBusManager, AzureServiceBusManager>();
            services.AddScoped<IServiceBus, AzureServiceBus>();

            services.AddHostedService<AzureServiceBusTopicListener>();
            services.AddHostedService<AzureServiceBusResponseListener>();

            return services;
        }

        public static ServiceBusOptions AddServiceBusOptions(this IServiceCollection services, IConfiguration configuration) {
            var options = GetServiceBusOptions(configuration);
            services.Configure<ServiceBusOptions>(c => {
                c.Enabled = options.Enabled;
                c.Provider = options.Provider;
                c.HostUrl = options.HostUrl;
                }
            );

            return options;
        }

        /// <summary>
        /// Add service bus core services, regardless of message broker provider
        /// </summary>
        /// <param name="services"></param>
        /// <param name="producers"></param>
        /// <param name="consumers"></param>
        private static void AddServiceBusCore(this IServiceCollection services, IEnumerable<Type> producers, IEnumerable<Type> consumers) {
            // Message registry
            var registry = new MessageRegistry<IRequest>();
            foreach (var producer in producers)
                registry.RegisterProducer(producer);
            foreach (var consumer in consumers)
                registry.RegisterConsumer(consumer);

            services.AddSingleton<IMessageRegistry<IRequest>>(registry);

            // Core infrastructure
            services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            services.AddSingleton<IMessageResponseTracker, InMemoryMessageResponseTracker>();
            services.AddSingleton<ReplyQueueResolver>();
        }

        private static ServiceBusOptions GetServiceBusOptions(IConfiguration configuration) {
            ServiceBusOptions serviceBusOptions = new ServiceBusOptions();
            configuration.GetSection("ServiceBus").Bind(serviceBusOptions);

            return serviceBusOptions;
        }
    }
}
