using AutoMapper;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Core.Domain.Entities.UserAggregate;

namespace CH.CleanArchitecture.Core.Application
{
    internal class UserProfile : Profile
    {
        public UserProfile() {
            CreateMap<UpdateUserDetailsCommand, UpdateUserDetailsDTO>();
            CreateMap<LoginUserCommand, LoginRequestDTO>();
            CreateMap<LoginUser2FACommand, Login2FARequestDTO>();
            CreateMap<User, UserReadModel>().ForMember(target => target.Roles, opt => opt.Ignore());
        }
    }
}