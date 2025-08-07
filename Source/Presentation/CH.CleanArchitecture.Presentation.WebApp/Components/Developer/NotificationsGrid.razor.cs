using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs.Notifications;
using CH.CleanArchitecture.Presentation.Framework.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using CH.CleanArchitecture.Common;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Developer
{
    public partial class NotificationsGrid : BaseComponent
    {
        [Inject] private INotificationService _notificationService { get; set; }
        private IEnumerable<NotificationDTO> _pagedData;
        private MudTable<NotificationDTO> _table;
        private int _totalItems;
        private string _searchString = null;

        private async Task<TableData<NotificationDTO>> ServerReload(TableState state, CancellationToken cancellationToken) {
            IQueryable<NotificationDTO> data = _notificationService.GetAll().Unwrap();
            await Task.Delay(300);
            if (!string.IsNullOrWhiteSpace(_searchString)) {
                data = data.Where(ac => ac.RecipientEmail.Contains(_searchString) || ac.RecipientPhone.Contains(_searchString));
            }

            _totalItems = data.Count();
            switch (state.SortLabel) {
                case nameof(NotificationDTO.Id):
                    data = data.OrderByDirection(state.SortDirection, o => o.Id);
                    break;
                case nameof(NotificationDTO.DateCreated):
                    data = data.OrderByDirection(state.SortDirection, o => o.DateCreated);
                    break;
            }

            _pagedData = data.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new TableData<NotificationDTO>() { TotalItems = _totalItems, Items = _pagedData };
        }

        private async Task OnSearch(string text) {
            _searchString = text;
            await _table.ReloadServerData();
        }
    }
}
