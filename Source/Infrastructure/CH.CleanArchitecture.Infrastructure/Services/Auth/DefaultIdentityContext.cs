using System.Collections.Generic;
using System.Security.Claims;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class DefaultIdentityContext : IIdentityContext
    {
        public string UserId => string.Empty;
        public string Username => string.Empty;
        public string Name => string.Empty;
        public string Culture => string.Empty;
        public string UiCulture => string.Empty;
        public List<string> Roles => new();
        public string Theme => string.Empty;
        public string ClientId => string.Empty;
        public List<ClaimData> Claims { get; set; } = new();
        public ClaimsPrincipal User => new ClaimsPrincipal();

        public void Initialize(List<ClaimData> claimData) {
            return;
        }
    }
}
