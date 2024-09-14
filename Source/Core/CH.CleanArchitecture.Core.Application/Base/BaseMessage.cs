using CH.Messaging.Abstractions;

namespace CH.CleanArchitecture.Core.Application
{
    public abstract class BaseMessage<TResponse> : IRequest<TResponse> where TResponse : class
    {
        public IIdentityProvider IdentityProvider { get; set; }
    }
}
