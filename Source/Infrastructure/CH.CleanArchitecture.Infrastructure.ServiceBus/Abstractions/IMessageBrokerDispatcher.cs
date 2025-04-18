using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    /// <summary>
    /// Abstracts the logic for sending/publishing messages to the underlying message broker.
    /// Responsible for sending commands with reply support and fire-and-forget events.
    /// </summary>
    public interface IMessageBrokerDispatcher
    {
        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : class;
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IRequest;
    }
}
