using System;
using System.Threading.Tasks;
using AutoMapper;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Authorization;
using CH.CleanArchitecture.Core.Application.DTOs;

namespace CH.CleanArchitecture.Core.Application.Commands
{
    public class UpdateUserDetailsCommand : BaseCommand<Result>
    {
        public UpdateUserDetailsCommand(string id) {
            Id = id;
            AddRequirements(UserOperations.Update(id));
        }

        public string Id { get; private set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string PrimaryPhone { get; set; }
        public string SecondaryPhone { get; set; }
        public string LanguagePreference { get; set; }
        public AddressDTO Address { get; set; }
    }

    /// <summary>
    /// Update User Details Command Handler
    /// </summary>
    public class UpdateUserDetailsCommandHandler : BaseCommandHandler<UpdateUserDetailsCommand, Result>
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly IMapper _mapper;

        public UpdateUserDetailsCommandHandler(IServiceProvider serviceProvider, IApplicationUserService applicationUserService, IMapper mapper) : base(serviceProvider) {
            _applicationUserService = applicationUserService;
            _mapper = mapper;
        }

        public override async Task<Result> HandleAsync(UpdateUserDetailsCommand command) {
            return await _applicationUserService.UpdateUserDetailsAsync(_mapper.Map<UpdateUserDetailsDTO>(command));
        }
    }
}