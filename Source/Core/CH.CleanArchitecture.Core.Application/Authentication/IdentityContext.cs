using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Core.Application
{
    public class IdentityContext : IIdentityContext
    {
        private const string FALLBACK_USERNAME = "SystemUser";

        public string UserId { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string ProfilePicture { get; set; }
        public string Culture { get; set; }
        public string UiCulture { get; set; }
        public List<RoleEnum> Roles { get; set; } = new();
        public ThemeEnum Theme { get; set; }
        public List<ClaimData> Claims { get; set; } = new();

        [JsonIgnore]
        public ClaimsPrincipal User { get; private set; }

        public IdentityContext() {
            Username = FALLBACK_USERNAME;
        }

        public void Initialize(List<ClaimData> claimData) {
            if (claimData == null || !claimData.Any()) {
                return;
            }
            Claims = claimData;

            List<Claim> claims = claimData.Select(c => new Claim(c.Type, c.Value)).ToList();
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "IdentityContext");
            var user = new ClaimsPrincipal(claimsIdentity);
            User = user;

            UserId = user.FindId() ?? null;
            Username = user.FindFullName() ?? null;
            Name = user.FindFullName() ?? null;
            Culture = user.FindCulture() ?? null;
            UiCulture = user.FindUiCulture() ?? null;
            Theme = user.FindTheme()?.ToEnum<ThemeEnum>() ?? default;

            var profilePictureClaim = user.FindProfilePicture();
            if (profilePictureClaim != null) {
                ProfilePicture = profilePictureClaim;
            }

            var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role);
            if (roles != null)
                Roles = roles.Select(r => r.Value.ToEnum<RoleEnum>()).ToList();
        }
    }

    public class ClaimData
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public ClaimData() { }
        public ClaimData(string type, string value) {
            Type = type;
            Value = value;
        }
    }
}