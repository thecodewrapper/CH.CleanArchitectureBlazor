﻿using System;
using CH.CleanArchitecture.Common;

namespace CH.CleanArchitecture.Core.Application.ReadModels
{
    public class NotificationReadModel : IReadModel
    {
        /// <summary>
        /// The notification unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Time notification created
        /// </summary>
        public DateTime DateCreated { get; set; }

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
        /// User the notification intended for
        /// </summary>
        public string UserFor { get; set; }

        /// <summary>
        /// The notification type (SMS, Email, Internal)
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Whether the notification has been sent successfully
        /// </summary>
        public bool IsSent { get; set; }
    }
}
