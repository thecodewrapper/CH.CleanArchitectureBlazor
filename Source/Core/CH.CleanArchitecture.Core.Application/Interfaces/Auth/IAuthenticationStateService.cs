using System.Security.Claims;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Core.Application
{
    public delegate Task CurrentUserChangedHandler();

    public interface IAuthenticationStateService
    {
        event CurrentUserChangedHandler CurrentUserChanged;

        Task<ClaimsPrincipal> GetCurrentUserAsync();

        Task SetCurrentUserAsync(ClaimsPrincipal user);
    }
}
