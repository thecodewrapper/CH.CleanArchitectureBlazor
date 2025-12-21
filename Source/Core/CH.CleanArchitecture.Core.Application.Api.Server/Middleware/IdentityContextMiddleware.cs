using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CH.CleanArchitecture.Core.Application
{
    public class IdentityContextMiddleware
    {
        private readonly RequestDelegate _next;

        public IdentityContextMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IIdentityContext identityContext) {
            if (identityContext != null) {
                ClaimsPrincipal user = context.User;
                if (user?.Identity?.IsAuthenticated == true) {
                    List<ClaimData> claims = user.Claims
                        .Select(c => new ClaimData { Type = c.Type, Value = c.Value })
                        .ToList();

                    identityContext.Initialize(claims);
                }
            }

            await _next(context);
        }
    }
}
