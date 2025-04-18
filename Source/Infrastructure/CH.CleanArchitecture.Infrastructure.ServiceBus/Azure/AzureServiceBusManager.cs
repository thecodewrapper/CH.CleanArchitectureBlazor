using System.Reflection;
using Azure.Messaging.ServiceBus.Administration;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    internal class AzureServiceBusManager : IServiceBusManager
    {
        private readonly IMessageRegistry<IRequest> _registry;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly string _appName;

        public AzureServiceBusManager(IMessageRegistry<IRequest> registry, ServiceBusAdministrationClient adminClient, IConfiguration configuration) {
            _registry = registry;
            _adminClient = adminClient;
            _appName = configuration["Application:Name"] ?? "app";
        }

        public void SubscribeToMessage<TMessage>() where TMessage : class, IRequest {
            if (!_registry.CanConsume(typeof(TMessage)))
                return;

            var topicName = TopicNameHelper.GetTopicName(typeof(TMessage));
            _ = EnsureTopicWithSubscriptionAsync<TMessage>(topicName, _appName);
        }

        public void SubscribeToMessagesFromAssembly(Assembly assembly) {
            var types = assembly.GetTypes()
                .Where(t => typeof(IRequest).IsAssignableFrom(t) && _registry.CanConsume(t));

            foreach (var type in types) {
                typeof(IServiceBusManager)
                    .GetMethod(nameof(SubscribeToMessage))!
                    .MakeGenericMethod(type)
                    .Invoke(this, null);
            }
        }

        private async Task EnsureTopicWithSubscriptionAsync<TMessage>(string topicName, string subscriptionName) {
            //Ensure topic exists
            if (!await _adminClient.TopicExistsAsync(topicName))
                await _adminClient.CreateTopicAsync(topicName);

            //Ensure subscription exists
            if (!await _adminClient.SubscriptionExistsAsync(topicName, subscriptionName)) {
                await _adminClient.CreateSubscriptionAsync(topicName, subscriptionName);

                //Filter to only receive messages of this type
                var rule = new CreateRuleOptions
                {
                    Name = "MessageTypeFilter",
                    Filter = new SqlRuleFilter($"[Type] = '{topicName}'")
                };

                await _adminClient.DeleteRuleAsync(topicName, subscriptionName, "$Default");
                await _adminClient.CreateRuleAsync(topicName, subscriptionName, rule);
            }
        }

        public async Task EnsureQueueExistsAsync(string queueName) {
            if (!await _adminClient.QueueExistsAsync(queueName))
                await _adminClient.CreateQueueAsync(queueName);
        }
    }
}
