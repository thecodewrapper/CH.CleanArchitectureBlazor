using System.Collections.Concurrent;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    /// <summary>
    /// Stores pending response tasks for messages awaiting a reply. Matches incoming
    /// responses by correlation ID and completes the appropriate TaskCompletionSource.
    /// </summary>
    public class InMemoryMessageResponseTracker : IMessageResponseTracker
    {
        private readonly ConcurrentDictionary<Guid, object> _pending = new();

        public Task<TResponse> WaitForResponseAsync<TResponse>(Guid correlationId, TimeSpan timeout) {
            var tcs = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[correlationId] = tcs;

            // Schedule timeout cleanup
            _ = Task.Delay(timeout).ContinueWith(_ =>
            {
                if (_pending.TryRemove(correlationId, out var existing)
                    && existing is TaskCompletionSource<TResponse> timedOutTcs) {
                    timedOutTcs.TrySetCanceled();
                }
            });

            return tcs.Task;
        }

        public void SetResponse<TResponse>(Guid correlationId, TResponse response) {
            if (_pending.TryRemove(correlationId, out var tcsObj)
                && tcsObj is TaskCompletionSource<TResponse> tcs) {
                tcs.TrySetResult(response!);
            }
            else {
                throw new InvalidOperationException($"No pending response tracker for correlation ID {correlationId}");
            }
        }
    }
}