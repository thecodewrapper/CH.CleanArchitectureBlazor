﻿using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application.Extensions;
using CH.CleanArchitecture.Core.Domain.User;
using Microsoft.AspNetCore.Authorization;

namespace CH.CleanArchitecture.Core.Application.Authorization
{
    /// <summary>
    /// Authorization handler for user operations. See <see cref="UserOperations"/>
    /// </summary>
    public class UserOperationAuthorizationHandler : AuthorizationHandler<UserOperationAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserOperationAuthorizationRequirement requirement) {
            switch (requirement.Name) {
                case OPERATIONS.USER.CREATE:
                case OPERATIONS.USER.UPDATE:
                case OPERATIONS.USER.DELETE:
                case OPERATIONS.USER.READ:
                case OPERATIONS.USER.CONFIRM_EMAIL:
                case OPERATIONS.USER.ACTIVATE:
                case OPERATIONS.USER.DEACTIVATE:
                    context.EvaluateRequirement(requirement, () => context.User.IsInRole(RoleEnum.Admin.ToString()));
                    break;
            }
            return Task.CompletedTask;
        }
    }
}