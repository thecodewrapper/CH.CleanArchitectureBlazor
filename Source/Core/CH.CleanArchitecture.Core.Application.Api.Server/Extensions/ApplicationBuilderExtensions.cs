using Microsoft.AspNetCore.Builder;

namespace CH.CleanArchitecture.Core.Application
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityContext(this IApplicationBuilder builder) {
            return builder.UseMiddleware<IdentityContextMiddleware>();
        }
    }
}
