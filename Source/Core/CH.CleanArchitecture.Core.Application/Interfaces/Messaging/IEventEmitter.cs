using System;
using System.Threading.Tasks;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IEventEmitter : IDisposable
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IRequest, IEvent;
    }
}
