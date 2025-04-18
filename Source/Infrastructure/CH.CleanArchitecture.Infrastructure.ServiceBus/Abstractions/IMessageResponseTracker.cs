namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    /// <summary>
    /// Tracks pending responses for messages that expect replies.
    /// Correlates received responses back to their original sender using CorrelationId.
    /// </summary>
    public interface IMessageResponseTracker
    {
        Task<TResponse> WaitForResponseAsync<TResponse>(Guid correlationId, TimeSpan timeout);
        void SetResponse<TResponse>(Guid correlationId, TResponse response);
    }
}