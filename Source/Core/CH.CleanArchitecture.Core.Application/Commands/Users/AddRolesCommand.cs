using CH.Messaging.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public record AddRolesCommand(string UserId, List<string> Roles) : IRequest<Result>, ICommand
    {
    }

    /// <summary>
    /// Add roles to user command handler
    /// </summary>
    public class AddRolesCommandHandler : BaseMessageHandler<AddRolesCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public AddRolesCommandHandler(IApplicationUserService applicationUserService) {
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