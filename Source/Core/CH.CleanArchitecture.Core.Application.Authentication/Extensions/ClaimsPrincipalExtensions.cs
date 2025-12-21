using CH.CleanArchitecture.Core.Application.Constants;
using System.Security.Claims;

namespace CH.CleanArchitecture.Core.Application
{
    public static class ClaimsPrincipalExtensions
    {
        public static string FindId(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.Id);
        }

        public static string FindClientId(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.Client.Id);
        }

        public static string FindClientEmail(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.Client.Email);
        }

        public static string FindClientName(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.Client.Name);
        }

        public static string FindClientOrigin(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.Client.Origin);
        }

        public static string FindFullName(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.FullName);
        }

        public static string FindUsername(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.Username);
        }

        public static string FindCulture(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.Culture);
        }

        public static string FindUiCulture(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.UiCulture);
        }

        public static string FindTheme(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.Theme);
        }

        public static string FindProfilePicture(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.ProfilePicture);
        }

        public static string FindMobilePhone(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ClaimTypes.MobilePhone);
        }

        private static string? FindFirstValue(this ClaimsPrincipal? cp, string claimType) => cp?.FindFirst(claimType)?.Value;
    }
}
