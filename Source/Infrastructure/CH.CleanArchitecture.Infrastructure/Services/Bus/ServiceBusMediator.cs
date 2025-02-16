using System;
using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Abstraction over the implementation specifics of a message broker transmission
    /// Concrete implementation of <see cref="IServiceBusMediator"/> which uses MassTransit's <see cref="IMediator"/>
    /// </summary>
    internal class ServiceBusMediator : IServiceBusMediator
    {
        private readonly ILogger<ServiceBusMediator> _logger;
        private readonly IMediator _mediator;
        private readonly IIdentityContext _identityContext;

        public ServiceBusMediator(ILogger<ServiceBusMediator> logger, IMediator mediator, IIdentityContext identityContext) {
            _logger = logger;
            _mediator = mediator;
            _identityContext = identityContext;
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : class {
            var client = _mediator.CreateRequestClient<IRequest<TResponse>>();
            cancellationToken.ThrowIfCancellationRequested();

            BaseMessage<TResponse> baseMessage = request as BaseMessage<TResponse>
                ?? throw new InvalidOperationException($"Request must be of type {nameof(BaseMessage<TResponse>)}");

            if (!baseMessage.IsBus) {
                baseMessage.IdentityContext = _identityContext as IdentityContext;
            }

            _logger.LogDebug("Sending message ({MessageType}) via MEDIATOR with correlation id {CorrelationId}. IsBus: {IsBus}", baseMessage.GetType().Name, baseMessage.CorrelationId, baseMessage.IsBus);
            try {
                var response = await client.GetResponse<TResponse>(baseMessage, cancellationToken);
                return response.Message;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error sending message to mediator");
                return default;
            }
        }

        public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            BaseMessage baseMessage = request as BaseMessage
                ?? throw new InvalidOperationException($"Request must be of type {nameof(BaseMessage)}");

            if (!baseMessage.IsBus) {
                baseMessage.IdentityContext = _identityContext as IdentityContext;
            }

            await _mediator.Send(request);
        }
    }
}