using CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.RabbitMQ
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, string hostUrl, IEnumerable<Type> producers, IEnumerable<Type> consumers) {
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMQConnectionManager>>();
                var serviceBusNaming = sp.GetRequiredService<IServiceBusNaming>();
                return new RabbitMQConnectionManager(hostUrl, logger, serviceBusNaming);
            });

            services.AddSingleton<IMessageBrokerManager, RabbitMQManager>();
            services.AddSingleton<IMessageBrokerDispatcher, RabbitMQMessageDispatcher>();

            services.AddHostedService<RabbitMQResponseListener>();
            services.AddHostedService<RabbitMQTopicListener>();

            return services;
        }
    }
}
