using System;
using CH.CleanArchitecture.Common;
using CH.Data.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.Models
{
    public class NotificationEntity : DataEntityBase<Guid>
    {
        /// <summary>
        /// A brief description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If is true then user has not yet see the notification
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Title of the notification
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Id of the recipient
        /// </summary>
        public string RecipientId { get; set; }

        /// <summary>
        /// Phone of the recipient
        /// </summary>
        public string RecipientPhone { get; set; }

        /// <summary>
        /// Email of the recipient
        /// </summary>
        public string RecipientEmail { get; set; }

        /// <summary>
        /// The notification type (SMS, Email, Internal)
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Whether the notification has been sent succesfully
        /// </summary>
        public bool IsSent { get; set; }
    }
}
