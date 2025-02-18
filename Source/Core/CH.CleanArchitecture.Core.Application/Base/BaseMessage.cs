using System;
using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Core.Application
{
    public abstract class BaseMessage<TResponse> : BaseMessage, IRequest<TResponse> where TResponse : class
    {
    }

    public abstract class BaseMessage : IRequest
    {
        public Guid CorrelationId { get; set; }
        public bool IsBus { get; set; }
        public bool IsEvent { get; set; }
        public string ResponseType { get; set; }
        public IdentityContext IdentityContext { get; set; }
    }
}
