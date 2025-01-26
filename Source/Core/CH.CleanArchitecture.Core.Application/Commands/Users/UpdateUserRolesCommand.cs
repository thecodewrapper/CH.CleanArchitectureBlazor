using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class UpdateUserRolesCommand : BaseCommand<Result>
    {
        public UpdateUserRolesCommand() {

        }

        public string Username { get; set; }
        public List<string> Roles { get; set; }
    }

    /// <summary>
    /// Update user roles command handler
    /// </summary>
    public class UpdateUserRolesCommandHandler : BaseCommandHandler<UpdateUserRolesCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public UpdateUserRolesCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
        }

        public override async Task<Result> HandleAsync(UpdateUserRolesCommand command) {
            return await _applicationUserService.UpdateRolesAsync(command.Username, command.Roles);
        }
    }
}