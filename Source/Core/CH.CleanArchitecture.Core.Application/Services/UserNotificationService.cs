using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs.Notifications;
using CH.CleanArchitecture.Core.Domain.User;
using Microsoft.Extensions.Logging;

namespace CH.CleanArchitecture.Core.Application.Services
{
    internal class UserNotificationService : IUserNotificationService
    {
        private readonly ILogger<UserNotificationService> _logger;
        private readonly INotificationService _notificationService;
        private readonly INotificationContentService _notificationContentService;

        public UserNotificationService(ILogger<UserNotificationService> logger, INotificationService notificationService, INotificationContentService notificationContentService) {
            _logger = logger;
            _notificationService = notificationService;
            _notificationContentService = notificationContentService;
        }

        public async Task NotifyUserForAccountConfirmationAsync(User user, string confirmationUrl) {
            ConfirmAccountDTO dto = new ConfirmAccountDTO()
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                ConfirmationUrl = confirmationUrl
            };

            var recipients = new List<NotificationRecipientDTO>() {
                new NotificationRecipientDTO()
                {
                    Id = user.Id,
                    Email = user.Email
                }
            };
            var emailContent = await _notificationContentService.GetConfirmAccountContent(NotificationType.Email, dto, new CultureInfo(user.UICulture));

            _logger.LogDebug($"Sending notification for account confirmation for user {user.Id}");

            await SendNotificationFromContent(emailContent, recipients);
        }

        public async Task NotifyUserForResetPasswordAsync(User user, string passwordResetUrl) {

            ResetPasswordDTO dto = new ResetPasswordDTO()
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                ResetPasswordUrl = passwordResetUrl
            };

            var recipients = new List<NotificationRecipientDTO>() {
                new NotificationRecipientDTO()
                {
                    Id = user.Id,
                    Email = user.Email
                }
            };
            var emailContent = await _notificationContentService.GetResetPasswordContent(NotificationType.Email, dto, new CultureInfo(user.UICulture));

            _logger.LogDebug($"Sending notification for password reset for user {user.Id}");

            await SendNotificationFromContent(emailContent, recipients);
        }

        private async Task SendNotificationFromContent(NotificationContentDTO content, List<NotificationRecipientDTO> recipients) {
            var notification = new SendNotificationDTO()
            {
                Title = content.Title,
                Message = content.Content,
                Recipients = recipients,
                Type = content.Type
            };

            _ = await _notificationService.SendNotificationAsync(notification);
        }
    }
}
