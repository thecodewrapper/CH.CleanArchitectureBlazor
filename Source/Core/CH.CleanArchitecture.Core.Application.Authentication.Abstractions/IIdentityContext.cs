using System.Security.Claims;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IIdentityContext
    {
        string UserId { get; }
        public string Username { get; }
        public string Name { get; }
        public string Culture { get; }
        public string UiCulture { get; }
        public List<string> Roles { get; }
        public string Theme { get; }
        public string ClientId { get; }
        public List<ClaimData> Claims { get; }
        public ClaimsPrincipal User { get; }

        void Initialize(List<ClaimData> claimData);
    }
}