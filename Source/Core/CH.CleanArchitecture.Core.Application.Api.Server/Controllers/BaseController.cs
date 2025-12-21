using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CH.CleanArchitecture.Common;
using System.Text.Json;

namespace CH.CleanArchitecture.Core.Application.Api
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IServiceBusMediator ServiceBus { get; }

        protected BaseController(IServiceBusMediator serviceBus) {
            ServiceBus = serviceBus;
        }

        protected IActionResult FromResult(Result result, int successStatusCode = 200) {
            if (result is null) {
                return StatusCode(500, new ApiResponsePayload
                {
                    Status = false,
                    Message = "Null result."
                });
            }

            if (result.IsSuccessful) {
                return StatusCode(successStatusCode, new ApiResponsePayload
                {
                    Status = true,
                    Message = string.IsNullOrWhiteSpace(result.Message)
                        ? "Success"
                        : result.Message,
                    Metadata = ToApiMetadata(result)
                });
            }

            var first = result.Errors?.FirstOrDefault();
            var status = ResultErrorHttpMapper.ToHttpStatus(first?.Code);
            var message = string.IsNullOrWhiteSpace(result.Message)
                ? first?.Error ?? "Request failed."
                : result.MessageWithErrors;

            return StatusCode(status, new ApiResponsePayload
            {
                Status = false,
                Message = message,
                Errors = result.Errors?
                    .Select(e => new ApiErrorDto { Code = e.Code, Error = e.Error })
                    .ToList(),
                Metadata = ToApiMetadata(result)
            });
        }

        protected IActionResult FromResult<T>(Result<T> result, bool created = false) {
            if (result is null) {
                return StatusCode(500, new ApiResponsePayload<T>
                {
                    Status = false,
                    Message = "Null result."
                });
            }

            if (result.IsSuccessful) {
                return StatusCode(
                    created ? StatusCodes.Status201Created : StatusCodes.Status200OK,
                    new ApiResponsePayload<T>
                    {
                        Status = true,
                        Message = string.IsNullOrWhiteSpace(result.Message)
                            ? created ? "Created" : "Success"
                            : result.Message,
                        Data = result.Data,
                        Metadata = ToApiMetadata(result)
                    });
            }

            var first = result.Errors?.FirstOrDefault();
            var status = ResultErrorHttpMapper.ToHttpStatus(first?.Code);
            var message = string.IsNullOrWhiteSpace(result.Message)
                ? first?.Error ?? "Request failed."
                : result.MessageWithErrors;

            return StatusCode(status, new ApiResponsePayload<T>
            {
                Status = false,
                Message = message,
                Errors = result.Errors?
                    .Select(e => new ApiErrorDto { Code = e.Code, Error = e.Error })
                    .ToList(),
                Metadata = ToApiMetadata(result)
            });
        }

        private static Dictionary<string, JsonElement>? ToApiMetadata(Result result) {
            if (result?.Metadata is null || result.Metadata.Count == 0)
                return null;

            var dict = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in result.Metadata) {
                // Serialize each value into a JsonElement (transport-safe)
                dict[kv.Key] = JsonSerializer.SerializeToElement(
                    kv.Value,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }

            return dict;
        }
    }
}