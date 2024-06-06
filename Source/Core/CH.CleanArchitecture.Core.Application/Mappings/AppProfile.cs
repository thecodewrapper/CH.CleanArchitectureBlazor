using AutoMapper;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Core.Application.Mappings
{
    internal class AppProfile : Profile
    {
        public AppProfile() {
            //Address
            CreateMap<AddressReadModel, AddressDTO>().ReverseMap();
        }
    }
}
