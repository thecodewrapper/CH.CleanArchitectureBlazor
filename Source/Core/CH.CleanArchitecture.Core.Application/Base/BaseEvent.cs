﻿using System;
using CH.CleanArchitecture.Common;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Core.Application
{
    public abstract class BaseEvent : BaseMessage<IResult>, IRequest<IResult>, IEvent
    {
        public Guid EventId => Guid.NewGuid();
    }
}
