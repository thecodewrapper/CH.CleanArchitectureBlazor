using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class AddRolesCommand : BaseCommand<Result>
    {
        public string UserId { get; }
        public List<string> Roles { get; }

        public AddRolesCommand(string userId, List<string> roles) {
            UserId = userId;
            Roles = roles ?? new List<string>();
        }
    }

    /// <summary>
    /// Add roles to user command handler
    /// </summary>
    public class AddRolesCommandHandler : BaseCommandHandler<AddRolesCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public AddRolesCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
        }

        public override async Task<Result> HandleAsync(AddRolesCommand command) {
            var request = new RoleAssignmentRequestDTO
            {
                UserId = command.UserId,
                Roles = command.Roles
            };

            return await _applicationUserService.AddRolesAsync(request);
        }
    }
}