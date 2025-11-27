using System;
using System.Collections.Generic;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IEventSink : IDisposable
    {
        void Subscribe<TEvent, TEventHandler>(IEnumerable<TEventHandler> handlers)
            where TEvent : class, IRequest, IEvent
            where TEventHandler : IHandler<TEvent>;

        void Subscribe<TEvent, TEventHandler>(TEventHandler handler)
            where TEvent : class, IRequest, IEvent
            where TEventHandler : IHandler<TEvent>;

        void Unsubscribe<TEvent, TEventHandler>(IEnumerable<TEventHandler> handlers)
            where TEvent : class, IRequest, IEvent
            where TEventHandler : IHandler<TEvent>;

        void Unsubscribe<TEvent, TEventHandler>(TEventHandler handler)
            where TEvent : class, IRequest, IEvent
            where TEventHandler : IHandler<TEvent>;
    }
}
