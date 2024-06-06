using System;

namespace CH.CleanArchitecture.Presentation.Framework.ViewModels
{
    public class NotificationViewModel
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
    }
}
