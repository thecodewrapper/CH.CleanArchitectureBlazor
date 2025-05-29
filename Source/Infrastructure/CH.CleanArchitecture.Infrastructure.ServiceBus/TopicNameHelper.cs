namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    internal static class TopicNameHelper
    {
        /// <summary>
        /// Returns the Azure Service Bus topic name based on the message type.
        /// </summary>
        public static string GetTopicName(Type type) { // e.g., LoginUserCommand
            return type.Name.ToLowerInvariant();
        }
    }
}