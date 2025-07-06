using CH.Messaging.Abstractions;
using System;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IEventEmitter : IDisposable
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IRequest, IEvent;
    }
}
