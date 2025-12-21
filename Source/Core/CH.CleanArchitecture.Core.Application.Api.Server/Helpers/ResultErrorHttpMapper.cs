namespace CH.CleanArchitecture.Core.Application.Api
{
    public static class ResultErrorHttpMapper
    {
        /// <summary>
        /// Decide HTTP status from the FIRST error code.
        /// Supports your numeric codes (e.g. "1","2") and a few common string fallbacks.
        /// </summary>
        public static int ToHttpStatus(string? code) {
            if (string.IsNullOrWhiteSpace(code))
                return 400;

            // Numeric codes you already use
            // 1 => Unauthorized, 2 => NotFound (from CommonResultErrors)
            switch (code) {
                case "1": return 401; // Unauthorized
                case "2": return 404; // Not Found
            }

            // Optional: friendly string codes if you ever emit them
            var c = code.Trim().ToLowerInvariant();
            return c switch
            {
                "unauthorized" or "auth_failed" => 401,
                "forbidden" => 403,
                "notfound" or "not_found" => 404,
                "conflict" => 409,
                "validation" or "invalid" => 422,
                "too_many_requests" => 429,
                "service_unavailable" => 503,
                _ => 400
            };
        }
    }
}
