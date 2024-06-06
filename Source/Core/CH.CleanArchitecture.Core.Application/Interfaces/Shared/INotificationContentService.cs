using System.Globalization;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.DTOs.Notifications;

namespace CH.CleanArchitecture.Core.Application
{
    /// <summary>
    /// Interface for service responsible for generating content (SMS, Email, Internal Message) for various notifications throughout the system.
    /// </summary>
    public interface INotificationContentService
    {
        ValueTask<NotificationContentDTO> GetConfirmAccountContent(NotificationType type, ConfirmAccountDTO dto, CultureInfo cultureInfo);
        ValueTask<NotificationContentDTO> GetResetPasswordContent(NotificationType type, ResetPasswordDTO dto, CultureInfo cultureInfo);
    }
}
