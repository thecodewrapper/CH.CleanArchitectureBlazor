using System.Text.Json;

namespace CH.CleanArchitecture.Core.Application.Api
{
    public static class ApiResponseExtensions
    {
        /// <summary>
        /// Returns Data if present, otherwise default(T).
        /// Safe for UI usage.
        /// </summary>
        public static T? Unwrap<T>(this ApiResponse<T> response) {
            if (response is null)
                return default;

            if (!response.IsSuccess)
                return default;

            return response.Payload.Data;
        }

        public static T GetMetadata<T>(
            this ApiResponse response,
            string key) {
            if (response.Payload.Metadata is null)
                throw new KeyNotFoundException(
                    $"Metadata collection is null. Key '{key}' was not found.");

            if (!response.Payload.Metadata.TryGetValue(key, out var element))
                throw new KeyNotFoundException(
                    $"Metadata key '{key}' was not found.");

            try {
                var value = element.Deserialize<T>(JsonOptions);
                if (value is null)
                    throw new InvalidOperationException(
                        $"Metadata key '{key}' exists but could not be deserialized to {typeof(T).Name}.");

                return value;
            }
            catch (Exception ex) {
                throw new InvalidOperationException(
                    $"Failed to deserialize metadata key '{key}' to {typeof(T).Name}.", ex);
            }
        }

        public static T GetMetadata<T ,TResponsePayload>(this ApiResponse<TResponsePayload> response, string key) {
            if (response.Payload.Metadata is null)
                throw new KeyNotFoundException(
                    $"Metadata collection is null. Key '{key}' was not found.");

            if (!response.Payload.Metadata.TryGetValue(key, out var element))
                throw new KeyNotFoundException(
                    $"Metadata key '{key}' was not found.");

            try {
                var value = element.Deserialize<T>(JsonOptions);
                if (value is null)
                    throw new InvalidOperationException(
                        $"Metadata key '{key}' exists but could not be deserialized to {typeof(T).Name}.");

                return value;
            }
            catch (Exception ex) {
                throw new InvalidOperationException(
                    $"Failed to deserialize metadata key '{key}' to {typeof(T).Name}.", ex);
            }
        }

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);
    }
}