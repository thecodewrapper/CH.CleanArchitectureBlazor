using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace CH.CleanArchitecture.Core.Application.Authorization
{
    public class UserOperationAuthorizationRequirement : OperationAuthorizationRequirement
    {
        public string UserId { get; private set; }
        public UserOperationAuthorizationRequirement() {

        }
        public UserOperationAuthorizationRequirement(string userId) {
            UserId = userId;
        }
    }
}
