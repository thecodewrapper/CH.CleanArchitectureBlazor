using System.Collections.Generic;
using System.Threading.Tasks;
using CH.Messaging.Abstractions;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public record RemoveRolesCommand(string UserId, List<string> Roles) : IRequest<Result>, ICommand
    {
    }

    /// <summary>
    /// Remove role from user command handler
    /// </summary>
    public class RemoveRolesCommandHandler : BaseMessageHandler<RemoveRolesCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public RemoveRolesCommandHandler(IApplicationUserService applicationUserService) {
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