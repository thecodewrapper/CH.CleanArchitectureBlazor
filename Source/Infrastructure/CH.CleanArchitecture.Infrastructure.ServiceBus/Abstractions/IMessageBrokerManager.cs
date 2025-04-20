namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public interface IMessageBrokerManager
    {
        Task CreateSubscriptionAsync(string topicName, string subscriptionName);
        Task CreateTopicAsync(string topicName);
        Task CreateQueueAsync(string queueName);
    }
}
