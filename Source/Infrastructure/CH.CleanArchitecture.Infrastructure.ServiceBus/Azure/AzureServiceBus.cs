using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    public class AzureServiceBus : IServiceBus, IEventBus
    {
        private readonly IServiceBusMediator _localMediator;
        private readonly IMessageBrokerDispatcher _brokerPublisher;
        private readonly IMessageRegistry<IRequest> _registry;
        private readonly IIdentityContext _identityContext;

        public AzureServiceBus(IServiceBusMediator localMediator, IMessageBrokerDispatcher brokerPublisher, IMessageRegistry<IRequest> registry, IIdentityContext identityContext) {
            _localMediator = localMediator;
            _brokerPublisher = brokerPublisher;
            _registry = registry;
            _identityContext = identityContext;
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
                    baseMessage.IdentityContext = _identityContext as IdentityContext;
                }

                return await _brokerPublisher.SendAsync(request, cancellationToken);
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

                await _brokerPublisher.PublishAsync(request, cancellationToken);
            }
            else {
                // In-process event only
                await _localMediator.PublishAsync(request, cancellationToken);
            }
        }
    }
}
