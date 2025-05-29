using CH.CleanArchitecture.Infrastructure.ServiceBus.Azure;
using CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddSingleton<IMessageBrokerManager, AzureServiceBusManager>();
            services.AddSingleton<IMessageBrokerDispatcher, AzureServiceBusMessageDispatcher>();

            services.AddHostedService<AzureServiceBusTopicListener>();
            services.AddHostedService<AzureServiceBusResponseListener>();

            return services;
        }

        public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, string hostUrl, IEnumerable<Type> producers, IEnumerable<Type> consumers) {
            services.AddServiceBusCore(producers, consumers);

            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMQConnectionManager>>();
                var serviceBusNaming = sp.GetRequiredService<ServiceBusNaming>();
                return new RabbitMQConnectionManager(hostUrl, logger, serviceBusNaming);
            });

            services.AddSingleton<IMessageBrokerManager, RabbitMQManager>();
            services.AddSingleton<IMessageBrokerDispatcher, RabbitMQMessageDispatcher>();

            services.AddHostedService<RabbitMQResponseListener>();
            services.AddHostedService<RabbitMQTopicListener>();

            return services;
        }

        public static ServiceBusOptions AddServiceBusOptions(this IServiceCollection services, IConfiguration configuration) {
            var options = GetServiceBusOptions(configuration);
            services.Configure<ServiceBusOptions>(c =>
            {
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
            services.AddSingleton<ServiceBusNaming>();
        }

        private static ServiceBusOptions GetServiceBusOptions(IConfiguration configuration) {
            ServiceBusOptions serviceBusOptions = new ServiceBusOptions();
            configuration.GetSection("ServiceBus").Bind(serviceBusOptions);

            return serviceBusOptions;
        }
    }
}
