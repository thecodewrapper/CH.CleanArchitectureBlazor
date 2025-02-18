using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IServiceBusManager
    {
        void SubscribeTo<TEvent>() where TEvent : IRequest;
    }
}
