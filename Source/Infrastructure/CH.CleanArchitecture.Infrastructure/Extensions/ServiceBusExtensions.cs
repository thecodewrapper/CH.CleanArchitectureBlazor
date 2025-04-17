using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

        internal static void AddAzureServiceBus(this IServiceCollection services, string hostUrl, List<Type> messageTypes, string appName) {
            Guard.Against.NullOrEmpty(hostUrl, nameof(hostUrl));

            services.AddMassTransit(cfg =>
            {
                // Register consumers
                RegisterConsumers(cfg, messageTypes);

                cfg.SetEndpointNameFormatter(new SimpleEndpointNameFormatter());
                //cfg.SetKebabCaseEndpointNameFormatter();
                cfg.UsingAzureServiceBus((context, config) =>
                {
                    config.UseServiceBusMessageScheduler();
                    config.AutoStart = true;
                    config.Host(hostUrl);

                    // Configure endpoints
                    ConfigureEndpoints(context, config, messageTypes, appName);
                });
            });
        }

        private static void RegisterConsumers(IBusRegistrationConfigurator cfg, IEnumerable<Type> messageTypes) {
            foreach (var messageType in messageTypes) {
                var consumerType = typeof(BusMessageConsumer<>).MakeGenericType(messageType);
                cfg.AddConsumer(consumerType);
            }
        }

        private static void ConfigureEndpoints(IBusRegistrationContext context, IServiceBusBusFactoryConfigurator config, IEnumerable<Type> messageTypes, string appName) {
            foreach (var messageType in messageTypes) {
                var consumerType = typeof(BusMessageConsumer<>).MakeGenericType(messageType);
                string topicName = messageType.Name.ToLowerInvariant();
                config.SubscriptionEndpoint(appName, topicName, e =>
                {
                    e.ConfigureConsumer(context, consumerType);
                });
            }
        }

        private class SimpleEndpointNameFormatter : IEndpointNameFormatter
        {
            private static readonly Regex _genericTypeRegex = new Regex(@"`\d+\[.*\]", RegexOptions.Compiled);

            public string Separator => throw new NotImplementedException();

            public string Message<T>() where T : class => SanitizeName(typeof(T).Name);

            public string SanitizeName(string name) {
                // Remove generics info if somehow present
                name = _genericTypeRegex.Replace(name, "");

                // Optionally remove suffixes
                name = name
                    .Replace("Command", "")
                    .Replace("Event", "")
                    .Replace("Query", "");

                // Optionally convert to kebab-case
                return ToKebabCase(name);
            }

            public string TemporaryEndpoint(string tag) => $"temporary-{SanitizeName(tag)}";

            string IEndpointNameFormatter.CompensateActivity<T, TLog>() => SanitizeName(typeof(T).Name);

            string IEndpointNameFormatter.Consumer<T>() => SanitizeName(typeof(T).Name);

            string IEndpointNameFormatter.ExecuteActivity<T, TArguments>() => SanitizeName(typeof(T).Name);

            string IEndpointNameFormatter.Saga<T>() => SanitizeName(typeof(T).Name);

            private static string ToKebabCase(string input) {
                return Regex.Replace(input, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
            }
        }
    }
}
