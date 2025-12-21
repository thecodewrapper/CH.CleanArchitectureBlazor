using System.Text.Json;

namespace CH.CleanArchitecture.Core.Application.Api
{
    public sealed record ApiResponse(ApiResponsePayload Payload, int StatusCode)
    {
        public bool IsSuccess => Payload?.Status == true;
        public IReadOnlyList<ApiErrorDto> Errors => Payload.Errors ?? new();
        public IReadOnlyDictionary<string, JsonElement> Metadata => Payload.Metadata
            ?? (IReadOnlyDictionary<string, JsonElement>)new Dictionary<string, JsonElement>();
    }

    public sealed record ApiResponse<T>(ApiResponsePayload<T> Payload, int StatusCode)
    {
        public bool IsSuccess => Payload?.Status == true;
        public T? Data => Payload.Data;
        public IReadOnlyList<ApiErrorDto> Errors => Payload.Errors ?? new();
        public IReadOnlyDictionary<string, JsonElement> Metadata => Payload.Metadata
            ?? (IReadOnlyDictionary<string, JsonElement>)new Dictionary<string, JsonElement>();
    }
}