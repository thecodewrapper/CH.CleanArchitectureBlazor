using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Authorization;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class DeactivateUserCommand : BaseCommand<Result>
    {
        public string Username { get; private set; }

        public DeactivateUserCommand(string username) {
            Username = username;

            AddRequirements(UserOperations.Deactivate);
        }
    }

    /// <summary>
    /// Deactivate User Command Handler
    /// </summary>
    public class DeactivateUserCommandHandler : BaseCommandHandler<DeactivateUserCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public DeactivateUserCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
        }

        public override async Task<Result> HandleAsync(DeactivateUserCommand command) {
            return await _applicationUserService.DeactivateUserAsync(command.Username);
        }
    }
}