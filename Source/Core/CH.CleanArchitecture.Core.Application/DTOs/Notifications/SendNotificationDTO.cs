using System.Collections.Generic;

namespace CH.CleanArchitecture.Core.Application.DTOs.Notifications
{
    public class SendNotificationDTO : SendNotificationBaseDTO
    {
        public List<NotificationRecipientDTO> Recipients { get; set; }
    }
}
