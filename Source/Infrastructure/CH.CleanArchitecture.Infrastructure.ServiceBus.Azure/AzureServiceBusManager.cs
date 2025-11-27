using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    /// <summary>
    /// Manages the creation and configuration of Azure Service Bus topics, queues, and subscriptions.
    /// </summary>
    internal class AzureServiceBusManager : IMessageBrokerManager
    {
        private const int REPLY_QUEUE_AUTO_DELETE_ON_IDLE_DAYS = 10;

        private readonly ILogger<AzureServiceBusManager> _logger;
        private readonly ServiceBusAdministrationClient _adminClient;

        private HashSet<string> _existingTopics = null;

        public AzureServiceBusManager(ILogger<AzureServiceBusManager> logger, ServiceBusAdministrationClient serviceBusAdministrationClient) {
            _logger = logger;
            _adminClient = serviceBusAdministrationClient;
        }

        public async Task CreateQueueAsync(string queueName) {
            if (!await _adminClient.QueueExistsAsync(queueName)) {
                await CreateQueue(queueName);
            }
        }

        public async Task CreateSubscriptionAsync(string topicName, string subscriptionName) {
            try {
                if (!await _adminClient.SubscriptionExistsAsync(topicName, subscriptionName)) {
                    await _adminClient.CreateSubscriptionAsync(topicName, subscriptionName);
                    _logger.LogInformation("Created subscription: {Subscription} on topic: {Topic}", subscriptionName, topicName);

                    var rule = new CreateRuleOptions
                    {
                        Name = "MessageTypeFilter",
                        Filter = new SqlRuleFilter($"[Type] = '{topicName}' AND ([Recipient] IS NULL OR [Recipient] = '{subscriptionName}')")
                    };

                    await _adminClient.DeleteRuleAsync(topicName, subscriptionName, "$Default");
                    await _adminClient.CreateRuleAsync(topicName, subscriptionName, rule);
                }
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists) {
                _logger.LogInformation("Subscription already exists: {Subscription} on topic: {Topic}", subscriptionName, topicName);
            }
        }

        public async Task CreateTopicAsync(string topicName) {
            var existingTopics = await GetExistingTopicsAsync();

            if (existingTopics.Contains(topicName)) {
                _logger.LogInformation("Topic already exists: {Topic}", topicName);
                return;
            }

            try {
                _logger.LogInformation("Attempting to create topic: {Topic}", topicName);
                await _adminClient.CreateTopicAsync(topicName);
                _logger.LogInformation("Created topic: {Topic}", topicName);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists) {
                _logger.LogInformation("Topic already exists: {Topic}", topicName);
            }
        }

        private async Task CreateQueue(string queueName) {
            var options = new CreateQueueOptions(queueName)
            {
                AutoDeleteOnIdle = TimeSpan.FromDays(REPLY_QUEUE_AUTO_DELETE_ON_IDLE_DAYS),
                MaxDeliveryCount = 10
            };

            try {
                _logger.LogInformation("Attempting to create reply queue '{QueueName}' with auto-delete on idle after {Days} day(s)", queueName, REPLY_QUEUE_AUTO_DELETE_ON_IDLE_DAYS);
                await _adminClient.CreateQueueAsync(options);
                _logger.LogInformation("Queue '{QueueName}' created successfully", queueName);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists) {
                _logger.LogInformation("Queue '{QueueName}' already exists. Skipping creation.", queueName);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error while ensuring queue '{QueueName}' exists.", queueName);
                throw;
            }
        }

        private async Task<HashSet<string>> GetExistingTopicsAsync() {
            if (_existingTopics != null) {
                return _existingTopics;
            }

            _existingTopics = new HashSet<string>();
            await foreach (var topic in _adminClient.GetTopicsAsync()) {
                _existingTopics.Add(topic.Name);
            }

            return _existingTopics;
        }
    }
}
