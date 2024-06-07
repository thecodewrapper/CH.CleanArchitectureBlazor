using CH.Messaging.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Domain.Entities.UserAggregate;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class CreateUserCommand : BaseCommand<Result>
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; }

        public CreateUserCommand() {

        }

        public CreateUserCommand(string username, string name, string surname, string email, string password, List<string> roles) {
            Username = username;
            Name = name;
            Surname = surname;
            Email = email;
            Password = password;
            Roles = roles;
        }
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