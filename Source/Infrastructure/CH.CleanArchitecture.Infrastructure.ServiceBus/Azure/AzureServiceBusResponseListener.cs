using Azure.Messaging.ServiceBus;
using CH.CleanArchitecture.Core.Application;
using Microsoft.Extensions.Hosting;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus.Azure
{
    /// <summary>
    /// Background service that listens to the reply queue and routes response messages
    /// back to their awaiting TaskCompletionSource via IResponseTracker.
    /// </summary>
    internal class AzureServiceBusResponseListener : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageResponseTracker _tracker;

        public AzureServiceBusResponseListener(ServiceBusClient serviceBusClient, IMessageSerializer serializer, IMessageResponseTracker tracker, ReplyQueueResolver replyQueueResolver) {
            _serializer = serializer;
            _tracker = tracker;
            _processor = serviceBusClient.CreateProcessor(replyQueueResolver.GetReplyQueueName(), new ServiceBusProcessorOptions());
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            _processor.ProcessMessageAsync += OnMessageReceived;
            _processor.ProcessErrorAsync += args => Task.CompletedTask;
            return _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task OnMessageReceived(ProcessMessageEventArgs args) {
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
        }
    }
}
