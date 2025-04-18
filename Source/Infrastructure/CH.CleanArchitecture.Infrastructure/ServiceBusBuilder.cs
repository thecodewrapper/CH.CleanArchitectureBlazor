using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.Extensions;
using CH.CleanArchitecture.Infrastructure.ServiceBus;
using CH.CleanArchitecture.Infrastructure.ServiceBus.Azure;
using CH.CleanArchitecture.Infrastructure.Services;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Infrastructure
{
    public class ServiceBusBuilder
    {
        private readonly IServiceCollection _services;
        private readonly List<Type> _mediatorConsumers = new();
        private readonly List<Assembly> _mediatorAssemblies = new();
        private readonly List<Type> _serviceBusMessageTypes = new();
        private readonly List<Assembly> _serviceBusAssemblies = new();
        private bool _useMediator = false;
        private bool _useServiceBus = false;
        private string _serviceBusProvider;
        private string _serviceBusHostUrl;
        private string _appName;

        public ServiceBusBuilder(IServiceCollection services) {
            _services = services;
        }

        public ServiceBusBuilder WithAppName(string appName) {
            _appName = appName.ToLowerInvariant();
            return this;
        }

        public ServiceBusBuilder UseMediator(params Type[] consumerTypes) {
            _useMediator = true;
            if (consumerTypes != null && consumerTypes.Any()) {
                _mediatorConsumers.AddRange(consumerTypes);
            }
            return this;
        }

        public ServiceBusBuilder UseMediator(IEnumerable<Assembly> assemblies) {
            _useMediator = true;
            if (assemblies != null && assemblies.Any()) {
                _mediatorAssemblies.AddRange(assemblies);
            }
            return this;
        }

        public ServiceBusBuilder UseServiceBus(string provider, string hostUrl, IEnumerable<Assembly> assemblies) {
            _useServiceBus = true;
            _serviceBusProvider = provider;
            _serviceBusHostUrl = hostUrl;

            if (assemblies != null && assemblies.Any()) {
                _serviceBusAssemblies.AddRange(assemblies);
            }
            UseMediator(assemblies);
            return this;
        }

        public void Build() {
            if (_useMediator) {
                BuildMediator();
            }

            if (_useServiceBus) {
                BuildServiceBus();
            }
            else {
                // If only mediator is used, register it as IServiceBus
                _services.AddScoped<IServiceBus, MassTransitMediator>();
            }
        }

        private void BuildMediator() {
            _services.AddScoped<IServiceBusMediator, MassTransitMediator>();
            _services.AddServiceBusMediator(_mediatorConsumers, _mediatorAssemblies);
        }

        private void BuildServiceBus() {
            _services.AddScoped<IServiceBus, AzureServiceBus>();
            _services.AddScoped<IEventBus, AzureServiceBus>();

            // Determine the message types (either provided or from assemblies)
            var resolvedMessageTypes = _serviceBusMessageTypes.Any() ? _serviceBusMessageTypes : GetMessageTypesFromAssemblies(_serviceBusAssemblies);

            switch (_serviceBusProvider.ToLower()) {
                case "azure":
                    _services.AddAzureServiceBusMessaging(_serviceBusHostUrl, resolvedMessageTypes.ToList(), resolvedMessageTypes.ToList());
                    break;
                default:
                    throw new NotSupportedException($"The service bus provider '{_serviceBusProvider}' is not supported.");
            }
        }

        private static IEnumerable<Type> GetMessageTypesFromAssemblies(IEnumerable<Assembly> assemblies) {
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsClass && !t.IsAbstract &&
                    (t.IsSubclassOfGeneric(typeof(BaseCommandHandler<,>)) ||
                     t.IsSubclassOfGeneric(typeof(BaseEventHandler<>))))
                .Select(handlerType => handlerType.BaseType.GetGenericArguments().First()); // TRequest
        }
    }
}
