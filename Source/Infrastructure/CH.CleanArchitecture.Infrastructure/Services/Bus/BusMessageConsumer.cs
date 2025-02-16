using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class BusMessageConsumer : IConsumer<BusMessage>
    {
        private readonly ILogger<BusMessageConsumer> _logger;
        private readonly IServiceBusMediator _mediator;
        private readonly ServiceBusJsonSerializerOptions _jsonSerializerOptions;

        public BusMessageConsumer(ILogger<BusMessageConsumer> logger, IServiceBusMediator mediator, ServiceBusJsonSerializerOptions jsonSerializerOptions) {
            _logger = logger;
            _mediator = mediator;
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public async Task Consume(ConsumeContext<BusMessage> context) {
            BusMessage message = context.Message;

            _logger.LogDebug("Consuming bus message with correlation id {CorrelationId}", message.CorrelationId);
            var messageType = Type.GetType(message.RequestType);
            if (messageType == null) {
                throw new InvalidOperationException($"Unable to find type: {message.RequestType}");
            }

            var messageResponseType = Type.GetType(message.ResponseType);
            if (messageResponseType == null) {
                throw new InvalidOperationException($"Unable to find type: {message.ResponseType}");
            }

            var deserializedMessage = JsonSerializer.Deserialize(context.Message.Data, messageType, _jsonSerializerOptions.Options);

            var sendAsyncMethod = typeof(IServiceBus).GetMethods()
                                                     .FirstOrDefault(m =>
                                                         m.Name == "SendAsync" &&
                                                         m.IsGenericMethod &&
                                                         m.GetParameters().Length == 2 &&
                                                         m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IRequest<>)
                                                     );
            if (sendAsyncMethod == null) {
                throw new InvalidOperationException("Unable to find SendAsync method.");
            }

            var genericMethod = sendAsyncMethod.MakeGenericMethod(messageResponseType);
            var task = (Task)genericMethod.Invoke(_mediator, new[] { deserializedMessage, CancellationToken.None });
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            var response = resultProperty?.GetValue(task);

            if (response != null) {
                await context.RespondAsync(response);
            }
        }
    }
}
