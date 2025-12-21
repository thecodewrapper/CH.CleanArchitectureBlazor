using System.Security.Claims;

namespace CH.CleanArchitecture.Core.Application
{
    public delegate Task CurrentUserChangedHandler();

    public interface IAuthenticationStateService
    {
        event CurrentUserChangedHandler CurrentUserChanged;

        Task<ClaimsPrincipal?> GetCurrentUserAsync();

        Task SetCurrentUserAsync(ClaimsPrincipal user);
    }
}
