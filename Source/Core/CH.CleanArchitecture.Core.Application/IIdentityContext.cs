using System.Collections.Generic;
using System.Security.Claims;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IIdentityContext
    {
        string UserId { get; }
        public string Username { get; }
        public string Name { get; }
        public string Culture { get; }
        public string UiCulture { get; }
        public List<RoleEnum> Roles { get; }
        public ThemeEnum Theme { get; }
        public List<ClaimData> Claims { get; }
        public ClaimsPrincipal User { get; }

        void Initialize(List<ClaimData> claimData);
    }
}