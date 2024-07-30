using Microsoft.AspNetCore.Components;
using MudBlazor;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Helpers;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Admin
{
    public partial class UsersGrid : BaseComponent
    {
        private IEnumerable<UserReadModel> _pagedData;
        private MudTable<UserReadModel> _table;
        private int _totalItems;
        private string _searchString = null;

        private async Task<TableData<UserReadModel>> ServerReload(TableState state) {
            QueryOptions options = TableHelper.GetQueryOptionsFromTableState(state, _searchString);
            GetAllUsersQuery getAllUsersQuery = new() { Options = options };

            var query = await SendRequestAsync(getAllUsersQuery);

            if (query.IsFailed) {
                ToastService.ShowError(query.MessageWithErrors);
                return new TableData<UserReadModel>();
            }

            _totalItems = (int)query.Metadata["AllRecordCount"];
            IEnumerable<UserReadModel> data = query.Unwrap();
            await Task.Delay(300);

            _pagedData = data;
            return new TableData<UserReadModel>() { TotalItems = _totalItems, Items = _pagedData };
        }

        private void OnSearch(string text) {
            _searchString = text;
            _table.ReloadServerData();
        }

        private void OnSelectedChipsChanged() {
            _table.ReloadServerData();
        }

        private async Task AddNewAsync() {
            IDialogReference modalRef = ModalService.ShowModal<UserForm>("Add user");
            Result result = await modalRef.GetReturnValueAsync<Result>();

            if (result?.IsSuccessful ?? false) {
                await _table.ReloadServerData();
            }
        }

        private void ShowUser(string userId) {
            NavigationManager.NavigateTo($"/admin/users/{userId}", false);
        }

        private async void DeleteUser(UserReadModel user) {
            bool confirmDelete = await ModalService.ShowConfirmationModalWithPhraseAsync("Delete user", $"Are you sure you want to delete user with id '{user.Id}'? This action cannot be undone and may result in unexpected system behaviour. Please consult with your administrator before proceeding.", "Delete", $"delete {user.Id}", Color.Error);

            if (confirmDelete) {
                var deleteResult = await SendRequestAsync(new DeleteUserCommand(user.Id));

                if (deleteResult != null && deleteResult.IsSuccessful) {
                    ToastService.ShowSuccess($"User with id '{user.Id}' deleted successfully.");
                    await _table.ReloadServerData();
                }
            }
        }
    }
}
