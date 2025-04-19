using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    internal class ServiceBus : IServiceBus, IEventBus
    {
        private readonly IServiceBusMediator _localMediator;
        private readonly IMessageBrokerDispatcher _brokerDispatcher;
        private readonly IMessageRegistry<IRequest> _registry;
        private readonly IIdentityContext _identityContext;
        private readonly ServiceBusNaming _serviceBusNaming;

        public ServiceBus(IServiceBusMediator localMediator, 
            IMessageBrokerDispatcher brokerDispatcher, 
            IMessageRegistry<IRequest> registry, 
            IIdentityContext identityContext, 
            ServiceBusNaming serviceBusNaming) {

            _localMediator = localMediator;
            _brokerDispatcher = brokerDispatcher;
            _registry = registry;
            _identityContext = identityContext;
            _serviceBusNaming = serviceBusNaming;
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            where TResponse : class {
            var messageType = request.GetType();

            if (_registry.CanProduce(messageType)) {
                // This is a bus-routed command
                if (request is BaseMessage<TResponse> baseMessage) {
                    baseMessage.IsBus = true;
                    baseMessage.IsEvent = false;
                    baseMessage.ResponseType = typeof(TResponse).AssemblyQualifiedName!;
                    baseMessage.CorrelationId = baseMessage.CorrelationId == Guid.Empty ? Guid.NewGuid() : baseMessage.CorrelationId;
                    baseMessage.InstanceId = _serviceBusNaming.GetInstanceId();
                    baseMessage.IdentityContext = _identityContext as IdentityContext;
                }

                return await _brokerDispatcher.SendAsync(request, cancellationToken);
            }

            // Fallback: local only (query or non-routed command)
            return await _localMediator.SendAsync(request, cancellationToken);
        }

        public async Task PublishAsync(IRequest request, CancellationToken cancellationToken = default) {
            var messageType = request.GetType();

            if (_registry.CanProduce(messageType)) {
                if (request is BaseMessage baseMessage) {
                    baseMessage.IsBus = true;
                    baseMessage.IsEvent = true;
                    baseMessage.IdentityContext = _identityContext as IdentityContext;
                }

                await _brokerDispatcher.PublishAsync(request, cancellationToken);
            }
            else                 // In-process event only
                await _localMediator.PublishAsync(request, cancellationToken);
        }
    }
}
