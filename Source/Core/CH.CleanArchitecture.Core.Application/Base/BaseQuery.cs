using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CH.CleanArchitecture.Core.Application
{
    public abstract class BaseQuery<TResponse> : BaseMessage<TResponse>, IQuery where TResponse : class
    {
        public List<IAuthorizationRequirement> Requirements { get; private set; } = new();

        public ClaimsPrincipal User { get; set; }

        protected void AddRequirements(params IAuthorizationRequirement[] requirements) {
            Requirements.AddRange(requirements);
        }
    }
}
