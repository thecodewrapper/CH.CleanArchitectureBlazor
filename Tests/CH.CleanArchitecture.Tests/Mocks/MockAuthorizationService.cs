using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CH.CleanArchitecture.Tests
{
    internal class MockAuthorizationService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements) {
            return Task.FromResult(AuthorizationResult.Success());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName) {
            return Task.FromResult(AuthorizationResult.Success());
        }
    }
}
