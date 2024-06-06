using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Authorization;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class ActivateUserCommand : BaseCommand<Result>
    {
        public string Username { get; private set; }

        public ActivateUserCommand(string username) {
            Username = username;

            AddRequirements(UserOperations.Activate);
        }
    }

    /// <summary>
    /// Activate User Command Handler
    /// </summary>
    public class ActivateUserCommandHandler : BaseCommandHandler<ActivateUserCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public ActivateUserCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
        }

        public override async Task<Result> HandleAsync(ActivateUserCommand command) {
            return await _applicationUserService.ActivateUserAsync(command.Username);
        }
    }
}