using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Abstraction over the implementation specifics of a message broker transmission
    /// Concrete implementation of <see cref="IServiceBus"/> which uses MassTransit's <see cref="IRequestClient<TResponse>"/>
    /// </summary>
    internal class MassTransitServiceBus : IServiceBus, IEventBus
    {
        private readonly ILogger<MassTransitServiceBus> _logger;
        private readonly IBus _bus;
        private readonly IIdentityContext _identityContext;
        private readonly IServiceProvider _serviceProvider;

        public MassTransitServiceBus(ILogger<MassTransitServiceBus> logger, IBus bus, IIdentityContext identityContext, IServiceProvider serviceProvider) {
            _logger = logger;
            _bus = bus;
            _identityContext = identityContext;
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : class {
            var clientType = typeof(IRequestClient<>).MakeGenericType(request.GetType());
            dynamic client = _serviceProvider.GetRequiredService(clientType);
            cancellationToken.ThrowIfCancellationRequested();

            Guid correlationId = Guid.NewGuid();
            BaseMessage<TResponse> baseMessage = request as BaseMessage<TResponse>
                ?? throw new InvalidOperationException($"Request must be of type {nameof(BaseMessage<TResponse>)}");
            baseMessage.IdentityContext = _identityContext as IdentityContext;
            baseMessage.CorrelationId = correlationId;
            baseMessage.IsBus = true;
            baseMessage.IsEvent = false;
            baseMessage.ResponseType = typeof(TResponse).AssemblyQualifiedName;

            _logger.LogDebug("Sending message ({MessageType}) via BUS with correlation id {CorrelationId}. IsBus: {IsBus}", baseMessage.GetType().Name, baseMessage.CorrelationId, baseMessage.IsBus);
            try {
                var response = await client.GetResponse<TResponse>(request, cancellationToken, RequestTimeout.Default);
                return response.Message;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error sending message to bus");
                return default;
            }
        }

        public async Task PublishAsync(IRequest request, CancellationToken cancellationToken = default) {
            Guid correlationId = Guid.NewGuid();
            BaseMessage baseMessage = request as BaseMessage
                ?? throw new InvalidOperationException($"Request must be of type {nameof(BaseMessage)}");
            baseMessage.IdentityContext = _identityContext as IdentityContext;
            baseMessage.CorrelationId = correlationId;
            baseMessage.IsBus = true;
            baseMessage.IsEvent = true;
            await _bus.Publish(request, cancellationToken);
        }
    }
}
