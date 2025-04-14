using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Tests.Mocks
{
    internal class MockServiceBus : IServiceBus
    {
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : class {
            return Task.FromResult<TResponse>(default);
        }
    }
}
