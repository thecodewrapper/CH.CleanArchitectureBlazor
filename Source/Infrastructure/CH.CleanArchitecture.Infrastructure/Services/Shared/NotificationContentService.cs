using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.DTOs.Notifications;
using CH.CleanArchitecture.Presentation.EmailTemplates;
using CH.CleanArchitecture.Presentation.EmailTemplates.Components.EN;
using CH.CleanArchitecture.Presentation.EmailTemplates.Components.GR;
using CH.CleanArchitecture.Presentation.EmailTemplates.Models;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class NotificationContentService : INotificationContentService
    {
        private readonly IComponentHtmlRenderer _componentHtmlRenderer;
        private readonly IEmailViewModelService _emailViewModelService;

        public NotificationContentService(IComponentHtmlRenderer componentHtmlRenderer, IEmailViewModelService emailViewModelService) {
            _componentHtmlRenderer = componentHtmlRenderer;
            _emailViewModelService = emailViewModelService;
        }

        public async ValueTask<NotificationContentDTO> GetConfirmAccountContent(NotificationType type, ConfirmAccountDTO dto, CultureInfo cultureInfo) {
            NotificationContentDTO contentDTO = new NotificationContentDTO(type);
            contentDTO.Title = "CH.CleanArchitecture - Confirm your account";
            switch (contentDTO.Type) {
                case NotificationType.Email: {
                        if (cultureInfo.TwoLetterISOLanguageName.ToLower() == "en") {
                            contentDTO.Content = await _componentHtmlRenderer.RenderComponentAsync<ConfirmAccountEN>(new Dictionary<string, object?>() { { nameof(ConfirmAccountEN.Model), ConstructEmailViewModel(dto) } });
                        }
                        else {
                            contentDTO.Content = await _componentHtmlRenderer.RenderComponentAsync<ConfirmAccountGR>(new Dictionary<string, object?>() { { nameof(ConfirmAccountGR.Model), ConstructEmailViewModel(dto) } });
                        }
                    }
                    ;
                    break;
                default: throw new NotImplementedException();
            }
            return contentDTO;
        }

        public async ValueTask<NotificationContentDTO> GetResetPasswordContent(NotificationType type, ResetPasswordDTO dto, CultureInfo cultureInfo) {
            NotificationContentDTO contentDTO = new NotificationContentDTO(type);
            contentDTO.Title = "CH.CleanArchitecture - Reset your password";
            switch (contentDTO.Type) {
                case NotificationType.Email: {
                        if (cultureInfo.TwoLetterISOLanguageName.ToLower() == "en") {
                            contentDTO.Content = await _componentHtmlRenderer.RenderComponentAsync<ResetPasswordEN>(new Dictionary<string, object?>() { { nameof(ResetPasswordEN.Model), ConstructEmailViewModel(dto) } });
                        }
                        else {
                            contentDTO.Content = await _componentHtmlRenderer.RenderComponentAsync<ResetPasswordGR>(new Dictionary<string, object?>() { { nameof(ResetPasswordGR.Model), ConstructEmailViewModel(dto) } });
                        }
                    }
                    ;
                    break;
                default: throw new NotImplementedException();
            }
            return contentDTO;
        }

        private EmailViewModel<T> ConstructEmailViewModel<T>(T payload) {
            return _emailViewModelService.ConstructEmailViewModel<T>(payload);
        }
    }
}
