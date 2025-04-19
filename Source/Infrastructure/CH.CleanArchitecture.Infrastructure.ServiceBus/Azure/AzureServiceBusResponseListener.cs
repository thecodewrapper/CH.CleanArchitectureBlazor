using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
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
        private const int REPLY_QUEUE_AUTO_DELETE_ON_IDLE_DAYS = 10;
        private readonly ServiceBusProcessor _processor;
        private readonly ILogger<AzureServiceBusResponseListener> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageResponseTracker _tracker;
        private readonly ServiceBusNaming _serviceBusNaming;
        private readonly string _replyQueueName;

        public AzureServiceBusResponseListener(
            ILogger<AzureServiceBusResponseListener> logger,
            ServiceBusClient serviceBusClient,
            ServiceBusAdministrationClient serviceBusAdministrationClient,
            IMessageSerializer serializer,
            IMessageResponseTracker tracker,
            ServiceBusNaming serviceBusNaming) {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _adminClient = serviceBusAdministrationClient;
            _serializer = serializer;
            _tracker = tracker;
            _serviceBusNaming = serviceBusNaming;
            _replyQueueName = serviceBusNaming.GetReplyQueueName();
            _processor = serviceBusClient.CreateProcessor(_replyQueueName, new ServiceBusProcessorOptions());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _logger.LogInformation($"Starting Azure Service Bus response listener with reply queue name: {_replyQueueName}");
            await EnsureQueueExistsAsync(_replyQueueName);

            _processor.ProcessMessageAsync += OnMessageReceived;
            _processor.ProcessErrorAsync += args => Task.CompletedTask;

            await _processor.StartProcessingAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken) {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
            await base.StopAsync(cancellationToken);
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

        private async Task EnsureQueueExistsAsync(string queueName) {
            if (!await _adminClient.QueueExistsAsync(queueName)) {
                var options = new CreateQueueOptions(queueName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromDays(REPLY_QUEUE_AUTO_DELETE_ON_IDLE_DAYS),
                    MaxDeliveryCount = 10
                };
                _logger.LogInformation("Creating queue '{QueueName}' with auto-delete on idle set to {AutoDeleteOnIdle} days", queueName, REPLY_QUEUE_AUTO_DELETE_ON_IDLE_DAYS);
                await _adminClient.CreateQueueAsync(options);
                _logger.LogInformation("Queue '{QueueName}' created successfully", queueName);
            }
        }
    }
}