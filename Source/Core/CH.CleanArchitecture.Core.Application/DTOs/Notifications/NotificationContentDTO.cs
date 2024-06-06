using CH.CleanArchitecture.Common;

namespace CH.CleanArchitecture.Core.Application.DTOs
{
    /// <summary>
    /// Used to encapsulate the content of a notification to be sent
    /// Used by NotificationContentService
    /// </summary>
    public class NotificationContentDTO
    {
        public NotificationContentDTO(NotificationType type) {
            Type = type;
        }
        public string Content { get; set; }
        public string Title { get; set; }
        public NotificationType Type { get; private set; }
    }
}
