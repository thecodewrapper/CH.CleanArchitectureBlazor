using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Authorization;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class DeleteUserCommand : BaseCommand<Result>
    {
        public string UserId { get; private set; }

        public DeleteUserCommand(string userId) {
            UserId = userId;

            AddRequirements(UserOperations.Delete);
        }
    }

    /// <summary>
    /// Delete User Command Handler
    /// </summary>
    public class DeleteUserCommandHandler : BaseCommandHandler<DeleteUserCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public DeleteUserCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
        }

        public async override Task<Result> HandleAsync(DeleteUserCommand command) {
            return await _applicationUserService.DeleteUserAsync(command.UserId);
        }
    }
}