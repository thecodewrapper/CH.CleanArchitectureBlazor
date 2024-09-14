using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class RemoveRolesCommand : BaseCommand<Result>
    {
        public Guid WorkspaceId { get; set; }
        public string UserId { get; set; }
        public List<string> Roles { get; set; }
    }

    /// <summary>
    /// Remove role from user command handler
    /// </summary>
    public class RemoveRolesCommandHandler : BaseCommandHandler<RemoveRolesCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public RemoveRolesCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
        }

        public override async Task<Result> HandleAsync(RemoveRolesCommand command) {
            var request = new RoleAssignmentRequestDTO
            {
                UserId = command.UserId,
                Roles = command.Roles
            };

            return await _applicationUserService.RemoveRolesAsync(request);
        }
    }
}