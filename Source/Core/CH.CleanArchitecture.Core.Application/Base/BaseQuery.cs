namespace CH.CleanArchitecture.Core.Application
{
    public abstract class BaseQuery<TResponse> : BaseMessage<TResponse>, IQuery where TResponse : class
    {
    }
}
