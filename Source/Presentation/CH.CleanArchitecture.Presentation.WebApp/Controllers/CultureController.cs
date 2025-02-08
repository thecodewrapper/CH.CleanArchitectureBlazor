using System.Security.Claims;
using CH.CleanArchitecture.Core.Application.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CH.CleanArchitecture.Presentation.WebApp.Controllers
{
    public class CultureController : Controller
    {
        public async Task<IActionResult> SetCulture(string culture, string redirectUri) {
            var user = HttpContext.User;

            if (user?.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
                return Unauthorized();

            // Update the "Language" claim
            var claims = new List<Claim>(identity.Claims);
            // Remove the old language claim if it exists
            claims.RemoveAll(c => c.Type == ApplicationClaimTypes.User.Culture);
            claims.RemoveAll(c => c.Type == ApplicationClaimTypes.User.UiCulture);

            // Add the new language claim
            claims.Add(new Claim(ApplicationClaimTypes.User.Culture, culture));
            claims.Add(new Claim(ApplicationClaimTypes.User.UiCulture, culture));

            // Create a new ClaimsPrincipal and update the cookie
            var newIdentity = new ClaimsIdentity(claims, identity.AuthenticationType);
            var newPrincipal = new ClaimsPrincipal(newIdentity);

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); // Remove the old cookie
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, newPrincipal); // Re-issue the new cookie

            return LocalRedirect(redirectUri);
        }
    }
}
