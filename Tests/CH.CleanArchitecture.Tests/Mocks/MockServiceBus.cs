using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Tests.Mocks
{
    internal class MockServiceBus : IServiceBusMediator, IServiceBus, IEventBus
    {
        public Task PublishAsync(IRequest request, CancellationToken cancellationToken = default) {
            return Task.CompletedTask;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : class {
            return Task.FromResult<TResponse>(default);
        }
    }
}
