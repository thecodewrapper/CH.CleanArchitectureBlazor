using System;
using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class LoginUserCommand : BaseCommand<Result<LoginResponseDTO>>
    {
        public string Username { get; init; }
        public string Password { get; init; }
        public bool RememberMe { get; init; }

        public LoginUserCommand(string username, string password, bool rememberMe) {
            Username = username;
            Password = password;
            RememberMe = rememberMe;
        }
    }

    /// <summary>
    /// Login User Command Handler
    /// </summary>
    public class LoginUserCommandHandler : BaseCommandHandler<LoginUserCommand, Result<LoginResponseDTO>>
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly IMapper _mapper;

        public LoginUserCommandHandler(IServiceProvider serviceProvider, IUserAuthenticationService userAuthenticationService, IMapper mapper) : base(serviceProvider) {
            _userAuthenticationService = userAuthenticationService;
            _mapper = mapper;
        }

        public override async Task<Result<LoginResponseDTO>> HandleAsync(LoginUserCommand command) {
            return await _userAuthenticationService.Login(_mapper.Map<LoginRequestDTO>(command));
        }
    }
}