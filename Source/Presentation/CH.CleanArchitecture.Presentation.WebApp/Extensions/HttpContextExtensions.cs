namespace CH.CleanArchitecture.Presentation.WebApp.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetBaseUri(this HttpContext httpContext) {
            var request = httpContext.Request;
            var baseUri = $"{request.Scheme}://{request.Host}{request.PathBase}/";

            return baseUri;
        }
    }
}
