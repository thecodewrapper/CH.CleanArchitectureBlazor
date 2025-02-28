using System;
using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Abstraction over the implementation specifics of a message broker transmission
    /// Concrete implementation of <see cref="IServiceBusMediator"/> which uses MassTransit's <see cref="IMediator"/>
    /// </summary>
    internal class MassTransitMediator : IServiceBusMediator
    {
        private readonly ILogger<MassTransitMediator> _logger;
        private readonly IMediator _mediator;
        private readonly IIdentityContext _identityContext;

        public MassTransitMediator(ILogger<MassTransitMediator> logger, IMediator mediator, IIdentityContext identityContext) {
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

            try {
                return await GetRetryPolicy().ExecuteAsync(async () =>
                {
                    _logger.LogDebug("Sending message ({MessageType}) via MEDIATOR with correlation id {CorrelationId}. IsBus: {IsBus}", baseMessage.GetType().Name, baseMessage.CorrelationId, baseMessage.IsBus);

                    var response = await client.GetResponse<TResponse>(baseMessage, cancellationToken);
                    return response.Message;
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to send message to MEDIATOR after retries.");
                return default;
            }
        }

        public async Task PublishAsync(IRequest request, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            BaseMessage baseMessage = request as BaseMessage
                ?? throw new InvalidOperationException($"Request must be of type {nameof(BaseMessage)}");

            if (!baseMessage.IsBus) {
                baseMessage.IdentityContext = _identityContext as IdentityContext;
            }

            try {
                await GetRetryPolicy().ExecuteAsync(async () =>
                {
                    _logger.LogDebug("Publishing message ({MessageType}) via MEDIATOR. IsBus: {IsBus}", baseMessage.GetType().Name, baseMessage.IsBus);
                    await _mediator.Publish(request, cancellationToken);
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to publish event to MEDIATOR after retries.");
            }
        }

        private AsyncRetryPolicy GetRetryPolicy() {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Retry {RetryCount} after {Delay}s due to exception: {Message}",
                            retryCount, timeSpan.TotalSeconds, exception.Message);
                    });
            return retryPolicy;
        }
    }
}