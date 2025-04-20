using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    /// <summary>
    /// Background service that listens to the reply queue and routes response messages
    /// back to their awaiting TaskCompletionSource via IResponseTracker.
    /// </summary>
    internal class AzureServiceBusResponseListener : BackgroundService
    {
        private ServiceBusProcessor? _processor;
        private readonly ILogger<AzureServiceBusResponseListener> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IMessageBrokerManager _manager;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageResponseTracker _tracker;
        private readonly ServiceBusNaming _serviceBusNaming;
        private readonly string _replyQueueName;

        public AzureServiceBusResponseListener(
            ILogger<AzureServiceBusResponseListener> logger,
            IMessageBrokerManager manager,
            IMessageSerializer serializer,
            IMessageResponseTracker tracker,
            ServiceBusClient serviceBusClient,
            ServiceBusNaming serviceBusNaming) {

            _logger = logger;
            _manager = manager;
            _serializer = serializer;
            _tracker = tracker;
            _serviceBusClient = serviceBusClient;
            _serviceBusNaming = serviceBusNaming;
            _replyQueueName = serviceBusNaming.GetReplyQueueName();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Starting Azure Service Bus response listener with reply queue name: {ReplyQueue}", _replyQueueName);
            try {
                await EnsureQueueExistsAsync();
                await StartProcessorAsync(cancellationToken);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound) {
                _logger.LogWarning(ex, "Reply queue '{Queue}' not found. Attempting to recreate and restart processor.", _replyQueueName);

                await EnsureQueueExistsAsync();
                await StartProcessorAsync(cancellationToken);
            }

            _logger.LogInformation("Azure Service Bus response listener started successfully.");
        }

        public override async Task StopAsync(CancellationToken cancellationToken) {
            if (_processor != null) {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }

        private async Task EnsureQueueExistsAsync() {
            await _manager.CreateQueueAsync(_replyQueueName);
        }

        private async Task StartProcessorAsync(CancellationToken cancellationToken) {
            _processor = _serviceBusClient.CreateProcessor(_replyQueueName, new ServiceBusProcessorOptions());
            _processor.ProcessMessageAsync += OnMessageReceived;
            _processor.ProcessErrorAsync += args => Task.CompletedTask;

            await _processor.StartProcessingAsync(cancellationToken);
            _logger.LogInformation("Service Bus response listener started for queue: {Queue}", _replyQueueName);
        }

        private async Task OnMessageReceived(ProcessMessageEventArgs args) {
            _logger.LogInformation("Received message on reply queue '{ReplyQueue}': {MessageId}", _replyQueueName, args.Message.MessageId);
            var raw = args.Message.Body.ToString();
            var correlationId = Guid.Parse(args.Message.CorrelationId);
            var responseType = Type.GetType(args.Message.Subject);
            if (responseType == null) return;
            var response = _serializer.Deserialize(raw, responseType);

            typeof(IMessageResponseTracker)
                .GetMethod(nameof(IMessageResponseTracker.SetResponse))!
                .MakeGenericMethod(responseType)
                .Invoke(_tracker, new[] { correlationId, response });

            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation($"Completed message with ID: {args.Message.MessageId}");
        }
    }
}