namespace CH.CleanArchitecture.Core.Application.Api
{
    public interface IApiClient
    {
        Task<ApiResponse<TResponse>> SendAsync<TResponse, TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : class, IApiRequest;
        public Task<ApiResponse> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : class, IApiRequest;
        Task<ApiResponse<TResponse>> GetAsync<TResponse, TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : class, IApiRequest;
        Task<ApiBinaryResponse> GetBinaryAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : class, IApiRequest;
    }
}
