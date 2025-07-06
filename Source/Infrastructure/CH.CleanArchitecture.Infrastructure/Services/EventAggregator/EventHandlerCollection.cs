using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class EventHandlerCollection<TEvent> where TEvent : class, IRequest, IEvent
    {
        private readonly List<IHandler<TEvent>> _handlers = new();
        private readonly object _lock = new();

        public void Add(IHandler<TEvent> handler) {
            lock (_lock) {
                if (!_handlers.Contains(handler))
                    _handlers.Add(handler);
            }
        }

        public void Remove(IHandler<TEvent> handler) {
            lock (_lock) {
                _handlers.Remove(handler);
            }
        }

        public async Task PublishAsync(TEvent @event) {
            List<IHandler<TEvent>> handlersCopy;
            lock (_lock) {
                handlersCopy = _handlers.ToList();
            }

            var tasks = handlersCopy.Select(h => h.HandleAsync(@event));
            await Task.WhenAll(tasks);
        }

        public void Clear() {
            lock (_lock) {
                _handlers.Clear();
            }
        }
    }
}
