using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        internal static void AddServiceBusMediator(this IServiceCollection services, List<Type> handlerTypes, List<Assembly> consumerAssemblies) {
            services.AddMediator(m =>
            {
                if (handlerTypes.Any()) {
                    handlerTypes.ForEach(consumerType => m.AddConsumer(consumerType));
                }

                if (consumerAssemblies.Any()) {
                    consumerAssemblies.ForEach(a => m.AddConsumers(a));
                }
            });
        }
    }
}
