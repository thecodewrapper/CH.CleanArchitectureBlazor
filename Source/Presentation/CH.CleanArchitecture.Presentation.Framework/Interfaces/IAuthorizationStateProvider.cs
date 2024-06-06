using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Presentation.Framework.Interfaces
{
    public interface IAuthorizationStateProvider
    {
        Task<bool> CheckRequirementAsync(ClaimsPrincipal user, IAuthorizationRequirement requirement);
        Task<bool> CheckRequirementAsync(ClaimsPrincipal user, object resource, IAuthorizationRequirement requirement);
    }
}
