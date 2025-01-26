using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CH.CleanArchitecture.Presentation.Framework.Interfaces
{
    public interface IAuthorizationStateProvider
    {
        Task<bool> CheckRequirementAsync(ClaimsPrincipal user, IAuthorizationRequirement requirement);
        Task<bool> CheckRequirementAsync(ClaimsPrincipal user, object resource, IAuthorizationRequirement requirement);
    }
}
