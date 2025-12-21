namespace CH.CleanArchitecture.Core.Application.Api
{
    public static class ApiResponseHelper
    {
        // 200 OK (default) or any custom 2xx you pass in.
        public static ApiResponse<T> Success<T>(T data, int statusCode = 200, string message = "Success") {
            var payload = new ApiResponsePayload<T>
            {
                Status = true,
                Message = message,
                Data = data
            };
            return new ApiResponse<T>(payload, statusCode);
        }

        // 200 OK (default) or any custom 2xx you pass in.
        public static ApiResponse Success(int statusCode = 200, string message = "Success") {
            var payload = new ApiResponsePayload
            {
                Status = true,
                Message = message,
            };
            return new ApiResponse(payload, statusCode);
        }

        // 201 Created
        public static ApiResponse<T> Created<T>(T data, string message = "Created") {
            var payload = new ApiResponsePayload<T>
            {
                Status = true,
                Message = message,
                Data = data
            };

            var response = new ApiResponse<T>(payload, 201);

            return response;
        }

        // 204 No Content (useful for DELETE or PATCH/PUT without body)
        public static ApiResponse NoContent(string message = "No Content") {
            var payload = new ApiResponsePayload
            {
                Status = true,
                Message = message
            };
            return new ApiResponse(payload, 204);
        }

        // 4xx/5xx failure wrapper (used in controller as Fail(...))
        public static ApiResponse Fail(string message, int statusCode = 400) {
            var payload = new ApiResponsePayload
            {
                Status = false,
                Message = message
            };
            return new ApiResponse(payload, statusCode);
        }

        // Backwards-compat: keep Error(...) but route it through Fail(...)
        public static ApiResponse Error(string message, int statusCode = 500)
            => Fail(message, statusCode);
    }
}