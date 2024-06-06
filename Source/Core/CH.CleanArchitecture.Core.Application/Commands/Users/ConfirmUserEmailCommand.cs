using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Authorization;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    /// <summary>
    /// Command specifically for manually confirming user email
    /// </summary>
    public class ConfirmUserEmailCommand : BaseCommand<Result>
    {
        public string UserId { get; private set; }

        public ConfirmUserEmailCommand(string userId) {
            UserId = userId;

            AddRequirements(UserOperations.ConfirmEmail(userId));
        }
    }

    /// <summary>
    /// Confirm user email Command Handler
    /// </summary>
    public class ConfirmUserEmailCommandHandler : BaseCommandHandler<ConfirmUserEmailCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public ConfirmUserEmailCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
        }

        public override async Task<Result> HandleAsync(ConfirmUserEmailCommand command) {
            return await _applicationUserService.ConfirmUserEmailAsync(command.UserId);
        }
    }
}
