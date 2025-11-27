using Azure.Messaging.ServiceBus;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    public interface IServiceBusReceiverPlugin
    {
        Task AfterMessageReceive(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default);
    }
}
