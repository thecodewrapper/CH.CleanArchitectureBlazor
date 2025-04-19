namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    /// <summary>
    /// Central registry of which message types an application is allowed to publish (produce)
    /// and/or handle (consume). Used by the bus infrastructure to validate messages.
    /// </summary>
    public class MessageRegistry<TMessage> : IMessageRegistry<TMessage> where TMessage : class
    {
        private readonly HashSet<Type> _producible = new();
        private readonly HashSet<Type> _consumable = new();

        public void RegisterProducer<T>() where T : TMessage => _producible.Add(typeof(T));
        public void RegisterConsumer<T>() where T : TMessage => _consumable.Add(typeof(T));

        public void RegisterProducer(Type messageType) {
            Validate(messageType);
            _producible.Add(messageType);
        }

        public void RegisterConsumer(Type messageType) {
            Validate(messageType);
            _consumable.Add(messageType);
        }

        public bool CanProduce(Type messageType) => _producible.Contains(messageType);
        public bool CanConsume(Type messageType) => _consumable.Contains(messageType);
        public IEnumerable<Type> GetConsumableTypes() => _consumable;
        public IEnumerable<Type> GetProducableTypes() => _producible;

        private static void Validate(Type messageType) {
            if (!typeof(TMessage).IsAssignableFrom(messageType))
                throw new InvalidOperationException($"{messageType.FullName} does not implement {typeof(TMessage).Name}.");
        }
    }
}
