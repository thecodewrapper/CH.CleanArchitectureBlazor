﻿using System;
using System.Threading.Tasks;
using CH.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace CH.CleanArchitecture.Core.Application
{
    /// <summary>
    /// Abstraction over a handler of messages coming from a message broker.
    /// Should implement any interfaces to comply with any given message brokers handler object
    /// Implemnents <see cref="IConsumer{TRequest}"/> for MassTransit implementation
    /// In case MassTransit is replaced with some other broker, then <see cref="IConsumer{TMessage}"/> should be changed to whatever interface needs to be implemented by the broker, or create another base message handler
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public abstract class BaseMessageHandler<TRequest, TResponse> : IHandler<TRequest, TResponse>, IConsumer<TRequest>
        where TRequest : BaseMessage<TResponse>
        where TResponse : class
    {
        protected BaseMessageHandler(IServiceProvider serviceProvider) {
            IdentityContext = serviceProvider.GetRequiredService<IIdentityContext>();
        }

        public virtual async Task Consume(ConsumeContext<TRequest> context) {
            IdentityContext.Initialize(context.Message.IdentityContext.Claims);
            var messageResult = await HandleAsync(context.Message);
            await context.RespondAsync(messageResult);
        }

        ///// <summary>
        ///// Method to be overriden by all command/query handlers
        ///// </summary>
        ///// <returns></returns>
        public abstract Task<TResponse> HandleAsync(TRequest request);

        protected IIdentityContext IdentityContext { get; set; }
    }
}
