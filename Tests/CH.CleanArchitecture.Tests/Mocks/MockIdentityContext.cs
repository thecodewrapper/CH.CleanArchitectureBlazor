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

        public IEnumerable<RoleEnum> Roles { get; set; } = new List<RoleEnum>();

        public string Culture => "en-GB";

        public string UiCulture => "en-GB";

        public ThemeEnum Theme => ThemeEnum.Dark;

        public ClaimsPrincipal User => new();

        public void Initialize(ClaimsPrincipal user) {
            return;
        }
    }
}
