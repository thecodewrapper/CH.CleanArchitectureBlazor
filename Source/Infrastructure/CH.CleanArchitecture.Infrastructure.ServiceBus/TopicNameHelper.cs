using CH.CleanArchitecture.Infrastructure.ServiceBus.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    internal class TopicNameHelper : ITopicNameFormatter
    {
        /// <summary>
        /// Returns the Azure Service Bus topic name based on the message type.
        /// </summary>
        public string GetTopicName(Type type) { // e.g., LoginUserCommand
            return type.Name.ToLowerInvariant();
        }
    }
}