namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions
{
    public interface IServiceBusNaming
    {
        string GetSubscriptionName();
        string GetReplyQueueName();
        Guid GetInstanceId();
    }
}
