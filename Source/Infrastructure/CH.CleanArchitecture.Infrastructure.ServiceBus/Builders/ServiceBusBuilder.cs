using System.Reflection;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public class ServiceBusBuilder
    {
        private readonly IServiceCollection _services;

        private MediatorOptions _mediatorOptions;
        private MessageBrokerOptions _brokerOptions;

        public ServiceBusBuilder(IServiceCollection services) {
            _services = services;
        }

        public ServiceBusBuilder UseMediator(Action<MediatorOptions> configure) {
            _mediatorOptions = new MediatorOptions();
            configure(_mediatorOptions);
            return this;
        }

        public ServiceBusBuilder UseServiceBus(string provider, string hostUrl, Action<MessageBrokerOptions> configure) {
            _brokerOptions = new MessageBrokerOptions(provider, hostUrl);
            configure(_brokerOptions);

            // Implicitly enable mediator
            _mediatorOptions ??= new MediatorOptions();
            _mediatorOptions.WithAssemblies(_brokerOptions.GetAssemblies().ToArray());

            return this;
        }

        public void Build() {
            if (_mediatorOptions != null)
                BuildMediator();

            if (_brokerOptions != null)
                BuildMessageBroker();
            else if (_mediatorOptions != null)
                _services.AddScoped<IServiceBus, MassTransitMediator>();
        }

        private void BuildMediator() {
            _services.AddScoped<IServiceBusMediator, MassTransitMediator>();
            _services.AddMediator(m =>
            {
                if (_mediatorOptions.GetConsumers().Any()) {
                    _mediatorOptions.GetConsumers().ForEach(consumerType => m.AddConsumer(consumerType));
                }

                if (_mediatorOptions.GetAssemblies().Any()) {
                    _mediatorOptions.GetAssemblies().ForEach(a => m.AddConsumers(a));
                }
            });
        }

        private void BuildMessageBroker() {
            var producers = _brokerOptions.GetProducers();
            var consumers = _brokerOptions.GetConsumers();
            var assemblies = _brokerOptions.GetAssemblies();
            var provider = _brokerOptions.Provider;
            var hostUrl = _brokerOptions.HostUrl;

            _services.AddScoped<IServiceBus, ServiceBus>();
            _services.AddScoped<IEventBus, ServiceBus>();

            if (!producers.Any() && !consumers.Any()) {
                var resolved = GetMessageTypesFromAssemblies(assemblies);
                producers = resolved.ToList();
                consumers = resolved.ToList();
            }

            switch (provider.ToLower()) {
                case "azure":
                    _services.AddAzureServiceBusMessaging(hostUrl, producers, consumers);
                    break;
                default:
                    throw new NotSupportedException($"Message broker '{provider}' is not supported.");
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
