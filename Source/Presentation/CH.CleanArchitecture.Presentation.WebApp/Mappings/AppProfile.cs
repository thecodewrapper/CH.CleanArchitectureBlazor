using AutoMapper;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Presentation.Framework.ViewModels;
using CH.CleanArchitecture.Presentation.WebApp.Models;

namespace CH.CleanArchitecture.Presentation.WebApp.Mappings
{
    internal class AppProfile : Profile
    {
        public AppProfile() {
            CreateMap<NotificationReadModel, NotificationViewModel>();
            CreateMap<ApplicationConfigurationDTO, ApplicationConfigurationFormModel>().ReverseMap();
            CreateMap<UserFormModel, CreateUserCommand>();
        }
    }
}