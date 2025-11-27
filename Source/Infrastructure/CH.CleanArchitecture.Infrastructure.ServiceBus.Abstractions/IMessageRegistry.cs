namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    /// <summary>
    /// Provides a registry of message types that the application is allowed to produce (send)
    /// and/or consume (handle). Used by the bus infrastructure to validate routing and subscriptions.
    /// </summary>
    public interface IMessageRegistry<TMessage> where TMessage : class
    {
        void RegisterProducer<T>() where T : TMessage;
        void RegisterConsumer<T>() where T : TMessage;

        void RegisterProducer(Type messageType);
        void RegisterConsumer(Type messageType);

        bool CanProduce(Type messageType);
        bool CanConsume(Type messageType);

        IEnumerable<Type> GetConsumableTypes();
        IEnumerable<Type> GetProducableTypes();
    }
}
