using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Helpers;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.User
{
    public partial class UserNotificationsGrid : BaseComponent
    {
        private IEnumerable<NotificationReadModel> _pagedData;
        private MudTable<NotificationReadModel> _table;
        private int _totalItems;
        private string _searchString = null;

        private async Task<TableData<NotificationReadModel>> ServerReload(TableState state, CancellationToken cancellationToken) {
            var user = await GetCurrentUserAsync();
            QueryOptions options = TableHelper.GetQueryOptionsFromTableState(state, _searchString);
            var query = await SendRequestAsync(new GetAllNotificationsQuery()
            {
                UserFor = user.FindId(),
                Type = NotificationType.Portal,
                Options = options
            });

            if (query.IsFailed) {
                ToastService.ShowError(query.MessageWithErrors);
                return new TableData<NotificationReadModel>();
            }

            _totalItems = Convert.ToInt32(query.Metadata["AllRecordCount"]);
            IEnumerable<NotificationReadModel> data = query.Unwrap();
            await Task.Delay(300);

            _pagedData = data;
            return new TableData<NotificationReadModel>() { TotalItems = _totalItems, Items = _pagedData };
        }

        private async Task OnSearch(string text) {
            _searchString = text;
            await _table.ReloadServerData();
        }
    }
}
