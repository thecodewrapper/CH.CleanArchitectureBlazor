using System.Reflection;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public interface IServiceBusManager
    {
        void SubscribeToMessage<TMessage>() where TMessage : class, IRequest;
        void SubscribeToMessagesFromAssembly(Assembly assembly);
        Task EnsureQueueExistsAsync(string queueName); //TODO[CH]: Should be in a different interface
    }
}
