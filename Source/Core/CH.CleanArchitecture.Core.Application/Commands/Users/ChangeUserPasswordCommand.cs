using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Authorization;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class ChangeUserPasswordCommand : BaseCommand<Result>
    {
        public string UserId { get; private set; }
        public string OldPassword { get; private set; }
        public string NewPassword { get; private set; }

        public ChangeUserPasswordCommand(string userId, string oldPassword, string newPassword) {
            UserId = userId;
            OldPassword = oldPassword;
            NewPassword = newPassword;

            AddRequirements(UserOperations.Update(userId));
        }
    }

    /// <summary>
    /// Change User Password Command Handler
    /// </summary>
    public class ChangeUserPasswordCommandHandler : BaseCommandHandler<ChangeUserPasswordCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public ChangeUserPasswordCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
        }

        public override async Task<Result> HandleAsync(ChangeUserPasswordCommand command) {
            return await _applicationUserService.ChangePasswordAsync(command.UserId, command.OldPassword, command.NewPassword);
        }
    }
}