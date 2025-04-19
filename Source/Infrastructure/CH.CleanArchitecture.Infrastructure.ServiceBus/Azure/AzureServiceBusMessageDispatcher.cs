using Azure.Messaging.ServiceBus;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    /// <summary>
    /// Responsible for sending command or event messages to Azure Service Bus topics.
    /// Supports request/response messaging via correlation ID and reply queues.
    /// </summary>
    internal class AzureServiceBusMessageDispatcher : IMessageBrokerDispatcher
    {
        private readonly ILogger<AzureServiceBusMessageDispatcher> _logger;
        private readonly ServiceBusClient _client;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageResponseTracker _tracker;
        private readonly string _replyTo;

        private const int WAIT_FOR_RESPONSE_TIMEOUT_SECONDS = 10;

        public AzureServiceBusMessageDispatcher(ILogger<AzureServiceBusMessageDispatcher> logger, ServiceBusClient client, IMessageSerializer serializer, IMessageResponseTracker tracker, ServiceBusNaming serviceBusNaming) {
            _logger = logger;
            _client = client;
            _serializer = serializer;
            _tracker = tracker;

            _replyTo = serviceBusNaming.GetReplyQueueName();
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : class {
            string topicName = TopicNameHelper.GetTopicName(request.GetType());
            Guid correlationId = (request as BaseMessage)?.CorrelationId ?? Guid.NewGuid();

            ServiceBusMessage message = ConstructServiceBusMessage(request, correlationId, topicName);
            ServiceBusSender sender = _client.CreateSender(topicName);

            _logger.LogDebug("Sending message ({MessageType}) to topic {TopicName} with correlation id {CorrelationId}.", request.GetType().Name, topicName, correlationId);
            await sender.SendMessageAsync(message, cancellationToken);
            return await _tracker.WaitForResponseAsync<TResponse>(correlationId, TimeSpan.FromSeconds(WAIT_FOR_RESPONSE_TIMEOUT_SECONDS));
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IRequest {
            string topicName = TopicNameHelper.GetTopicName(@event.GetType());

            ServiceBusMessage message = ConstructServiceBusMessage(@event);
            ServiceBusSender sender = _client.CreateSender(topicName);

            _logger.LogDebug("Publishing event ({MessageType}) to topic {TopicName}.", @event.GetType().Name, topicName);
            await sender.SendMessageAsync(message, cancellationToken);
        }

        private ServiceBusMessage ConstructServiceBusMessage<TResponse>(IRequest<TResponse> request, Guid correlationId, string topicName) where TResponse : class {
            var body = new BinaryData(_serializer.Serialize(request));
            var message = new ServiceBusMessage(body)
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyTo,
                Subject = typeof(TResponse).AssemblyQualifiedName,
                ApplicationProperties =
                {
                    ["Type"] = topicName
                }
            };

            return message;
        }

        private ServiceBusMessage ConstructServiceBusMessage<TEvent>(TEvent @event) where TEvent : class, IRequest {
            var body = new BinaryData(_serializer.Serialize(@event));
            var message = new ServiceBusMessage(body)
            {
                Subject = @event.GetType().AssemblyQualifiedName
            };
            return message;
        }
    }
}
