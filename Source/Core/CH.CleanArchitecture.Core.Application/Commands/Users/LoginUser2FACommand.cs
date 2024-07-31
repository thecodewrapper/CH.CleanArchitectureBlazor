using System;
using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class LoginUser2FACommand : BaseCommand<Result<LoginResponseDTO>>
    {
        public string Code { get; private set; }
        public bool IsPersisted { get; set; }
        public bool RememberClient { get; set; }

        /// <summary>
        /// Indicates whether this will be a 2FA login using a recovery code
        /// </summary>
        public bool IsRecoveryCode { get; set; }

        public LoginUser2FACommand(string code, bool isRecoveryCode) {
            Code = code;
            IsRecoveryCode = isRecoveryCode;
        }
    }

    /// <summary>
    /// Activate User Command Handler
    /// </summary>
    public class LoginUser2FACommandHandler : BaseCommandHandler<LoginUser2FACommand, Result<LoginResponseDTO>>
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly IMapper _mapper;

        public LoginUser2FACommandHandler(IServiceProvider serviceProvider, IUserAuthenticationService userAuthenticationService, IMapper mapper) : base(serviceProvider) {
            _userAuthenticationService = userAuthenticationService;
            _mapper = mapper;
        }

        public override async Task<Result<LoginResponseDTO>> HandleAsync(LoginUser2FACommand command) {
            var dto = _mapper.Map<Login2FARequestDTO>(command);

            if (command.IsRecoveryCode) {
                return await _userAuthenticationService.LoginWithRecoveryCode(dto);
            }

            return await _userAuthenticationService.LoginWith2fa(_mapper.Map<Login2FARequestDTO>(command));
        }
    }
}