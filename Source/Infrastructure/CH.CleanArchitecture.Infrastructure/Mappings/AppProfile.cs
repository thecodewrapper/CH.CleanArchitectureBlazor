using AutoMapper;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.DTOs.Notifications;
using CH.CleanArchitecture.Core.Application.Mappings;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Core.Domain;
using CH.CleanArchitecture.Infrastructure.Auditing;
using CH.CleanArchitecture.Infrastructure.Models;

namespace CH.CleanArchitecture.Infrastructure.Mappings
{
    internal class AppProfile : Profile
    {
        public AppProfile() {

            #region AuditHistory
            CreateMap<AuditHistory, AuditHistoryDTO>();
            CreateMap<AuditHistoryDetails, AuditHistoryDetailsDTO>();
            #endregion

            #region ApplicationConfigurations
            CreateMap<ApplicationConfigurationEntity, ApplicationConfigurationDTO>().ReverseMap();
            #endregion ApplicationConfigurations

            #region Notifications
            CreateMap<NotificationEntity, NotificationDTO>().ReverseMap();
            CreateMap<NotificationDTO, NotificationReadModel>();
            #endregion Notifications

            #region Common
            CreateMap<string, PhoneNumber>().ConvertUsing<StringToPhoneNumberConverter>();
            CreateMap<PhoneNumber, string>().ConvertUsing<PhoneNumberToStringConverter>();
            CreateMap<Address, AddressEntity>().ReverseMap();
            CreateMap<Address, AddressReadModel>();
            CreateMap<AddressEntity, AddressReadModel>();
            #endregion

            #region Resources
            CreateMap<ResourceEntity, ResourceDTO>();
            #endregion Resources
        }
    }
}
