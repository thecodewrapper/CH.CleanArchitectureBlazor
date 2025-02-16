using System.Collections.Generic;
using System.Security.Claims;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class DefaultIdentityContext : IIdentityContext
    {
        public string UserId => string.Empty;
        public string Username => string.Empty;
        public string Name => string.Empty;
        public string Culture => string.Empty;
        public string UiCulture => string.Empty;
        public List<RoleEnum> Roles => new();
        public ThemeEnum Theme => ThemeEnum.Light;
        public List<ClaimData> Claims { get; set; } = new();
        public ClaimsPrincipal User => new ClaimsPrincipal();

        public void Initialize(List<ClaimData> claimData) {
            return;
        }
    }
}
