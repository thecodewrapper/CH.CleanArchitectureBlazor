using System.Security.Claims;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Tests.Mocks
{
    internal class MockIdentityContext : IIdentityContext
    {
        public string UserId => "TestingUser";

        public string Username => "Mr.Tester";

        public string Name => "Tester Tester";

        public List<RoleEnum> Roles => new();

        public string Culture => "en-GB";

        public string UiCulture => "en-GB";

        public ThemeEnum Theme => ThemeEnum.Dark;

        public ClaimsPrincipal User => new();

        public List<ClaimData> Claims => new();

        public void Initialize(List<ClaimData> claimData) {
            return;
        }
    }
}
