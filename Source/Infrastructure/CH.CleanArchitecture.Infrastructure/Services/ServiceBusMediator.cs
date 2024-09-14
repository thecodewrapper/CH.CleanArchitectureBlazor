using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using MassTransit.Mediator;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Abstraction over the implementation specifics of a message broker transmission
    /// Concrete implementation of <see cref="IServiceBus"/> which uses MassTransit's <see cref="IMediator"/>
    /// </summary>
    public class ServiceBusMediator : IServiceBus, IEventBus
    {
        private readonly IMediator _mediator;
        private readonly IIdentityProvider _identityProvider;

        public ServiceBusMediator(IMediator mediator, IIdentityProvider identityProvider) {
            _mediator = mediator;
            _identityProvider = identityProvider;
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : class {
            var client = _mediator.CreateRequestClient<IRequest<TResponse>>();
            cancellationToken.ThrowIfCancellationRequested();
            var baseMessage = request as BaseMessage<TResponse>;
            baseMessage.IdentityProvider = _identityProvider;
            var response = await client.GetResponse<TResponse>(baseMessage, cancellationToken);
            return response.Message;
        }

        public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default) {
            await _mediator.Send(request);
        }
    }
}