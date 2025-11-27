using CH.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public static class ServiceCollectionExtensions
    {
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

        public static IServiceCollection AddMessageSerializer(this IServiceCollection services) {
            services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();

            return services;
        }

        /// <summary>
        /// Add service bus core services, regardless of message broker provider
        /// </summary>
        /// <param name="services"></param>
        /// <param name="producers"></param>
        /// <param name="consumers"></param>
        public static void AddServiceBusCore(this IServiceCollection services, IEnumerable<Type> producers, IEnumerable<Type> consumers) {
            // Message registry
            var registry = new MessageRegistry<IRequest>();
            foreach (var producer in producers)
                registry.RegisterProducer(producer);
            foreach (var consumer in consumers)
                registry.RegisterConsumer(consumer);

            services.AddSingleton<IMessageRegistry<IRequest>>(registry);

            // Core infrastructure
            services.AddMessageSerializer();
            services.AddSingleton<IMessageResponseTracker, InMemoryMessageResponseTracker>();
            services.AddSingleton<IServiceBusNaming, ServiceBusNaming>();
            services.AddSingleton<ITopicNameFormatter, TopicNameHelper>();
        }

        private static ServiceBusOptions GetServiceBusOptions(IConfiguration configuration) {
            ServiceBusOptions serviceBusOptions = new ServiceBusOptions();
            configuration.GetSection("ServiceBus").Bind(serviceBusOptions);

            return serviceBusOptions;
        }
    }
}
