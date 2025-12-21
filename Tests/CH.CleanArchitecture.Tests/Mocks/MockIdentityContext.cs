using System.Security.Claims;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Tests.Mocks
{
    internal class MockIdentityContext : IdentityContext, IIdentityContext
    {
        public new string UserId => "TestingUser";

        public new string Username => "Mr.Tester";

        public new string Name => "Tester Tester";

        public new List<RoleEnum> Roles => new();

        public new string Culture => "en-GB";

        public new string UiCulture => "en-GB";

        public new ThemeEnum Theme => ThemeEnum.Dark;

        public new ClaimsPrincipal User => new();

        public new List<ClaimData> Claims => new();

        public new void Initialize(List<ClaimData> claimData) {
            return;
        }
    }
}
