﻿using CH.Messaging.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Domain.Entities.UserAggregate;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public record CreateUserCommand(string Username, string Name, string Surname, string Email, string Password, List<string> Roles) : IRequest<Result>, ICommand
    {
    }

    /// <summary>
    /// Create User Command Handler
    /// </summary>
    public class CreateUserCommandHandler : BaseMessageHandler<CreateUserCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;

        public CreateUserCommandHandler(IApplicationUserService applicationUserService) {
            _applicationUserService = applicationUserService;
        }

        public async override Task<Result> HandleAsync(CreateUserCommand command) {
            var user = new User(command.Username, command.Email, command.Name, command.Surname);
            return await _applicationUserService.CreateUserAsync(user, command.Password, command.Roles);
        }
    }
}