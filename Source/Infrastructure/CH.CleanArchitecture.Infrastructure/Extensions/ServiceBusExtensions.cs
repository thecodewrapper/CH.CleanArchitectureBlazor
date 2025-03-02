using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ardalis.GuardClauses;
using CH.CleanArchitecture.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Infrastructure.Extensions
{
    public static class ServiceBusExtensions
    {
        public static void AddServiceBus(this IServiceCollection services, Action<ServiceBusBuilder> configure) {
            var builder = new ServiceBusBuilder(services);
            configure(builder);

            builder.Build();
        }

        internal static void AddServiceBusMediator(this IServiceCollection services, List<Type> consumerTypes, List<Assembly> consumerAssemblies) {
            services.AddMediator(m =>
            {
                if (consumerTypes.Any()) {
                    consumerTypes.ForEach(consumerType => m.AddConsumer(consumerType));
                }

                if (consumerAssemblies.Any()) {
                    consumerAssemblies.ForEach(a => m.AddConsumers(a));
                }
            });
        }

        internal static void AddAzureServiceBus(this IServiceCollection services, string hostUrl, List<Type> messageTypes) {
            Guard.Against.NullOrEmpty(hostUrl, nameof(hostUrl));

            services.AddMassTransit(cfg =>
            {
                // Register consumers
                RegisterConsumers(cfg, messageTypes);

                cfg.SetKebabCaseEndpointNameFormatter();
                cfg.UsingAzureServiceBus((context, config) =>
                {
                    config.UseServiceBusMessageScheduler();
                    config.AutoStart = true;
                    config.Host(hostUrl);

                    // Configure endpoints
                    ConfigureEndpoints(context, config, messageTypes);
                });
            });
        }

        private static void RegisterConsumers(IBusRegistrationConfigurator cfg, IEnumerable<Type> messageTypes) {
            foreach (var eventType in messageTypes) {
                var consumerType = typeof(BusMessageConsumer<>).MakeGenericType(eventType);
                cfg.AddConsumer(consumerType);
            }
        }

        private static void ConfigureEndpoints(IBusRegistrationContext context, IReceiveConfigurator config, IEnumerable<Type> messageTypes) {
            foreach (var eventType in messageTypes) {
                var consumerType = typeof(BusMessageConsumer<>).MakeGenericType(eventType);

                config.ReceiveEndpoint(eventType.Name.ToLower(), e =>
                {
                    e.ConfigureConsumer(context, consumerType);
                });
            }
        }
    }
}
