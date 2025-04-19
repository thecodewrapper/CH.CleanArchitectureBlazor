﻿using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    internal class ServiceBusNaming
    {
        private readonly string _subscriptionName;
        private readonly string _replyQueueName;
        private readonly Guid _instanceId;

        public ServiceBusNaming(IConfiguration configuration) {
            _subscriptionName = configuration["Application:Name"] ?? Assembly.GetEntryAssembly().GetName().FullName.ToLowerInvariant();
            _instanceId = Guid.NewGuid();
            _replyQueueName = $"{_subscriptionName}.replies-{_instanceId}";
        }

        public string GetSubscriptionName() => _subscriptionName;
        public string GetReplyQueueName() => _replyQueueName;
        public Guid GetInstanceId() => _instanceId;
    }
}
