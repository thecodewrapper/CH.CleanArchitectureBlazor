using CH.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    internal class ServiceBus : IServiceBus, IEventBus
    {
        private readonly ILogger<ServiceBus> _logger;
        private readonly IServiceBusMediator _localMediator;
        private readonly IMessageBrokerDispatcher _brokerDispatcher;
        private readonly IMessageRegistry<IRequest> _registry;
        private readonly IIdentityContext _identityContext;
        private readonly IServiceBusNaming _serviceBusNaming;

        public ServiceBus(ILogger<ServiceBus> logger,
            IServiceBusMediator localMediator,
            IMessageBrokerDispatcher brokerDispatcher,
            IMessageRegistry<IRequest> registry,
            IIdentityContext identityContext,
            IServiceBusNaming serviceBusNaming) {

            _logger = logger;
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
                BaseMessage<TResponse> baseMessage = request as BaseMessage<TResponse> ?? throw new InvalidOperationException($"Request must be of type {nameof(BaseMessage<TResponse>)}");

                baseMessage.IsBus = true;
                baseMessage.IsEvent = false;
                baseMessage.ResponseType = typeof(TResponse).AssemblyQualifiedName!;
                baseMessage.CorrelationId = baseMessage.CorrelationId == Guid.Empty ? Guid.NewGuid() : baseMessage.CorrelationId;
                baseMessage.InstanceId = _serviceBusNaming.GetInstanceId();
                baseMessage.IdentityContext = _identityContext as IdentityContext;

                _logger.LogDebug("Sending message ({MessageType}) via SERVICE BUS with correlation id {CorrelationId}. IsBus: {IsBus}", baseMessage.GetType().Name, baseMessage.CorrelationId, baseMessage.IsBus);
                return await _brokerDispatcher.SendAsync(request, cancellationToken);
            }

            // Fallback: local only (query or non-routed command)
            return await _localMediator.SendAsync(request, cancellationToken);
        }

        public async Task PublishAsync(IRequest request, CancellationToken cancellationToken = default) {
            var messageType = request.GetType();

            if (_registry.CanProduce(messageType)) {
                BaseMessage baseMessage = request as BaseMessage ?? throw new InvalidOperationException($"Request must be of type {nameof(BaseMessage)}");
                baseMessage.IsBus = true;
                baseMessage.IsEvent = true;
                baseMessage.CorrelationId = baseMessage.CorrelationId == Guid.Empty ? Guid.NewGuid() : baseMessage.CorrelationId;
                baseMessage.InstanceId = _serviceBusNaming.GetInstanceId();
                baseMessage.IdentityContext = _identityContext as IdentityContext;

                _logger.LogDebug("Publishing message ({MessageType}) via SERVICE BUS with correlation id {CorrelationId}. IsBus: {IsBus}", baseMessage.GetType().Name, baseMessage.CorrelationId, baseMessage.IsBus);
                await _brokerDispatcher.PublishAsync(request, cancellationToken);
            }
            else {
                // In-process event only
                await _localMediator.PublishAsync(request, cancellationToken);
            }
        }
    }
}
