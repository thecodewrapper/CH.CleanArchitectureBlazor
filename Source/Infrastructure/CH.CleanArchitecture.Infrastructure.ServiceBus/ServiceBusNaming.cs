using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    internal class ServiceBusNaming
    {
        private readonly string _subscriptionName;
        private readonly string _replyQueueName;

        public ServiceBusNaming(IConfiguration configuration) {
            _subscriptionName = configuration["Application:Name"] ?? Assembly.GetEntryAssembly().GetName().FullName.ToLowerInvariant();
            _replyQueueName = $"{_subscriptionName}.replies";
        }

        public string GetSubscriptionName() => _subscriptionName;

        public string GetReplyQueueName() => _replyQueueName;
    }
}
