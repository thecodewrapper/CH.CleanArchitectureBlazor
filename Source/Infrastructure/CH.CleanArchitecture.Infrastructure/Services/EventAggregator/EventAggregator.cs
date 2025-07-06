using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, object> _handlers = new();

        public void Subscribe<TEvent, TEventHandler>(IEnumerable<TEventHandler> handlers) where TEvent : class, IRequest, IEvent where TEventHandler : IHandler<TEvent> {
            foreach (var handler in handlers) {
                Subscribe<TEvent, TEventHandler>(handler);
            }
        }

        public void Subscribe<TEvent, TEventHandler>(TEventHandler handler) where TEvent : class, IRequest, IEvent where TEventHandler : IHandler<TEvent> {
            var collection = (EventHandlerCollection<TEvent>)_handlers.GetOrAdd(
                typeof(TEvent),
                _ => new EventHandlerCollection<TEvent>()
            );
            collection.Add(handler);
        }

        public void Unsubscribe<TEvent, TEventHandler>(IEnumerable<TEventHandler> handlers) where TEvent : class, IRequest, IEvent where TEventHandler : IHandler<TEvent> {
            foreach (var handler in handlers) {
                Unsubscribe<TEvent, TEventHandler>(handler);
            }
        }

        public void Unsubscribe<TEvent, TEventHandler>(TEventHandler handler) where TEvent : class, IRequest, IEvent where TEventHandler : IHandler<TEvent> {
            if (_handlers.TryGetValue(typeof(TEvent), out var obj) &&
                obj is EventHandlerCollection<TEvent> collection) {
                collection.Remove(handler);
            }
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IRequest, IEvent {
            if (_handlers.TryGetValue(typeof(TEvent), out var obj) &&
                obj is EventHandlerCollection<TEvent> collection) {
                await collection.PublishAsync(@event);
            }
        }

        public void Dispose() {
            foreach (var kv in _handlers.Values) {
                var clearMethod = kv.GetType().GetMethod("Clear");
                clearMethod?.Invoke(kv, null);
            }
            _handlers.Clear();
        }
    }
}
