using System.Collections.Generic;
using System.Security.Claims;
using CH.CleanArchitecture.Common;
using CH.Messaging.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace CH.CleanArchitecture.Core.Application
{
    public abstract class BaseCommand<TResponse> : BaseMessage<TResponse>, IRequest<TResponse>, ICommand
        where TResponse : class, IResult
    {
        public List<IAuthorizationRequirement> Requirements { get; private set; } = new();
        public ClaimsPrincipal User { get; set; }

        protected void AddRequirements(params IAuthorizationRequirement[] requirements) {
            Requirements.AddRange(requirements);
        }
    }
}
