using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Abstraction over the implementation specifics of a message broker transmission
    /// Concrete implementation of <see cref="IServiceBus"/> which uses MassTransit's <see cref="IRequestClient<TResponse>"/>
    /// </summary>
    internal class ServiceBus : IServiceBus, IEventBus
    {
        private readonly ILogger<ServiceBus> _logger;
        private readonly IBus _bus;
        private readonly IIdentityContext _identityContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusJsonSerializerOptions _serializerOptions;

        public ServiceBus(ILogger<ServiceBus> logger, IBus bus, IIdentityContext identityContext, IServiceProvider serviceProvider, ServiceBusJsonSerializerOptions serializerOptions) {
            _logger = logger;
            _bus = bus;
            _identityContext = identityContext;
            _serviceProvider = serviceProvider;
            _serializerOptions = serializerOptions;
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : class {
            var client = _serviceProvider.GetRequiredService<IRequestClient<BusMessage>>();
            cancellationToken.ThrowIfCancellationRequested();

            Guid correlationId = Guid.NewGuid();
            BaseMessage<TResponse> baseMessage = request as BaseMessage<TResponse>
                ?? throw new InvalidOperationException($"Request must be of type {nameof(BaseMessage<TResponse>)}");
            baseMessage.IdentityContext = _identityContext as IdentityContext;
            baseMessage.CorrelationId = correlationId;
            baseMessage.IsBus = true;

            string data = JsonSerializer.Serialize((object)request, request.GetType(), _serializerOptions.Options);
            var busMessage = new BusMessage()
            {
                Data = data,
                CorrelationId = correlationId,
                RequestType = request.GetType().AssemblyQualifiedName,
                ResponseType = typeof(TResponse).AssemblyQualifiedName
            };

            try {
                var response = await client.GetResponse<TResponse>(busMessage, cancellationToken, RequestTimeout.Default);
                return response.Message;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error sending message to bus");
                return default;
            }
        }

        public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default) {
            await _bus.Send(request);
        }
    }
}
