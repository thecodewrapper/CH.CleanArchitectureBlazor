using System.Collections.Generic;
using System;
using CH.CleanArchitecture.Core.Application;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class ServiceBusManager : IServiceBusManager
    {
        public HashSet<Type> EventTypes { get; } = new();

        public void SubscribeTo<TEvent>() where TEvent : IRequest {
            EventTypes.Add(typeof(TEvent));
        }
    }
}