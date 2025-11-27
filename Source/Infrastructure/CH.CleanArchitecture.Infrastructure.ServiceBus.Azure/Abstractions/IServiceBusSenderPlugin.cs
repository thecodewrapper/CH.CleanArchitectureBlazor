using Azure.Messaging.ServiceBus;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    public interface IServiceBusSenderPlugin
    {
        Task BeforeMessageSend(ServiceBusMessage message, CancellationToken cancellationToken = default);
    }
}
