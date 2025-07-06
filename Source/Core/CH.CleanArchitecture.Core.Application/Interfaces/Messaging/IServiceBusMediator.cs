using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IServiceBusMediator : IServiceBus, IEventBus
    {
    }
}