using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.Framework.ViewModels;
using Microsoft.AspNetCore.Components;

namespace CH.CleanArchitecture.Presentation.Framework.Components
{
    public partial class NotificationsPanel : BaseComponent
    {
        [Parameter]
        public List<NotificationViewModel> Notifications { get; set; } = new();

        [Inject]
        public INotificationService NotificationService { get; set; }

        [Inject]
        public ILocalizationService LocalizationService { get; set; }

        public bool _isOpen;

        public NotificationPanel() {
        }

        public void ToggleOpen() {
            if (_isOpen)
                _isOpen = false;
            else
                _isOpen = true;
        }

        private async Task MarkAllAsRead() {
            foreach (var notification in Notifications) {
                notification.IsNew = false;
                await NotificationService.MarkAsReadAsync(notification.Id);
            }
        }

        private async Task MarkAsRead(NotificationViewModel notification) {
            notification.IsNew = false;
            await NotificationService.MarkAsReadAsync(notification.Id);
        }
    }
}
