using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using MassTransit;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class BusMessageConsumer<TEvent> : IConsumer<TEvent> where TEvent : class, IRequest
    {
        private readonly IServiceBusMediator _mediator;

        public BusMessageConsumer(IServiceBusMediator mediator) {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<TEvent> context) {
            //check if its request so that we do _mediator.SendAsync or if its an event so that publish
            var message = context.Message as BaseMessage;
            if (message.IsEvent) {
                await _mediator.PublishAsync(context.Message, context.CancellationToken);
            }
            else {
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

                var messageType = context.Message.GetType();
                var messageResponseType = Type.GetType(message.ResponseType);
                if (messageResponseType == null) {
                    throw new InvalidOperationException($"Unable to find type: {message.ResponseType}");
                }

                var genericMethod = sendAsyncMethod.MakeGenericMethod(messageResponseType);
                var castedMessage = Convert.ChangeType(context.Message, messageType);
                var task = (Task)genericMethod.Invoke(_mediator, new[] { castedMessage, CancellationToken.None });
                await task.ConfigureAwait(false);

                var resultProperty = task.GetType().GetProperty("Result");
                var response = resultProperty?.GetValue(task);

                if (response != null) {
                    await context.RespondAsync(response);
                }
            }
        }
    }
}
