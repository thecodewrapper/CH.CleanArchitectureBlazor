namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public interface IMessageSerializer
    {
        string Serialize(object obj);
        object Deserialize(string data, Type type);
    }
}