using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Helpers;
using CH.CleanArchitecture.Resources;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Admin
{
    public partial class ResourcesGrid : BaseComponent
    {
        private IEnumerable<ResourceReadModel> _pagedData;
        private MudTable<ResourceReadModel> _table;
        private int _totalItems;
        private string _searchString = null;

        private async Task<TableData<ResourceReadModel>> ServerReload(TableState state, CancellationToken cancellationToken) {
            QueryOptions options = TableHelper.GetQueryOptionsFromTableState(state, _searchString);
            GetAllResourcesQuery query = new() { Options = options };

            var result = await SendRequestAsync(query);

            if (result.IsFailed) {
                ToastService.ShowError(result.MessageWithErrors);
                return new TableData<ResourceReadModel>();
            }

            _totalItems = Convert.ToInt32(result.Metadata["AllRecordCount"]);
            IEnumerable<ResourceReadModel> data = result.Unwrap();
            await Task.Delay(300);

            _pagedData = data;
            return new TableData<ResourceReadModel>() { TotalItems = _totalItems, Items = _pagedData };
        }

        private void OnSearch(string text) {
            _searchString = text;
            _table.ReloadServerData();
        }

        private async Task AddNewAsync() {
            IDialogReference modalRef = ModalService.ShowModal<ResourceForm>(LocalizationService.GetLocalizedString(ResourceKeys.Messages_AddResource));
            Result result = await modalRef.GetReturnValueAsync<Result>();

            if (result?.IsSuccessful ?? false) {
                await _table.ReloadServerData();
            }
        }

        private async void Delete(ResourceReadModel model) {
            bool confirmDelete = await ModalService.ShowConfirmationModalAsync(
                LocalizationService.GetLocalizedString(ResourceKeys.Messages_DeleteResource),
                LocalizationService.GetLocalizedString(ResourceKeys.Messages_DeleteResourceConfirmation, model.Name),
                LocalizationService.GetLocalizedString(ResourceKeys.Buttons_Delete_Cap),
                Color.Error);

            if (confirmDelete) {
                var deleteResult = await SendRequestAsync(new DeleteResourceCommand(model.Id));

                if (deleteResult != null && deleteResult.IsSuccessful) {
                    ToastService.ShowSuccess(deleteResult.Message);
                    await _table.ReloadServerData();
                }
                else {
                    ToastService.ShowError(deleteResult.MessageWithErrors);
                }
            }
        }
    }
}
