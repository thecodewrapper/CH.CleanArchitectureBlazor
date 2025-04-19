﻿using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
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
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageResponseTracker _tracker;
        private readonly ServiceBusNaming _serviceBusNaming;

        public AzureServiceBusResponseListener(
            ServiceBusClient serviceBusClient,
            ServiceBusAdministrationClient serviceBusAdministrationClient,
            IMessageSerializer serializer,
            IMessageResponseTracker tracker,
            ServiceBusNaming serviceBusNaming) {

            _serviceBusClient = serviceBusClient;
            _adminClient = serviceBusAdministrationClient;
            _serializer = serializer;
            _tracker = tracker;
            _serviceBusNaming = serviceBusNaming;
            _processor = serviceBusClient.CreateProcessor(_serviceBusNaming.GetReplyQueueName(), new ServiceBusProcessorOptions());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await EnsureQueueExistsAsync(_serviceBusNaming.GetReplyQueueName());

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

        private async Task EnsureQueueExistsAsync(string queueName) {
            if (!await _adminClient.QueueExistsAsync(queueName))
                await _adminClient.CreateQueueAsync(queueName);
        }
    }
}