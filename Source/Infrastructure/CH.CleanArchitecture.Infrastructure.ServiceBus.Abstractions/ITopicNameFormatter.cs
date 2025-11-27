namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions
{
    public interface ITopicNameFormatter
    {
        string GetTopicName(Type type);
    }
}
