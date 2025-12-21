using System.Text.Json;

namespace CH.CleanArchitecture.Core.Application.Api
{
    public class ApiResponsePayload<T> : ApiResponsePayload
    {
        public T? Data { get; set; }
    }

    public class ApiResponsePayload
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public List<ApiErrorDto>? Errors { get; set; }
        public Dictionary<string, JsonElement>? Metadata { get; set; }
    }
}
