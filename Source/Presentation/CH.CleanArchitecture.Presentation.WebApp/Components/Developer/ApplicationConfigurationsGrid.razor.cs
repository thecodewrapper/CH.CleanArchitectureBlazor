using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Presentation.Framework.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Developer
{
    public partial class ApplicationConfigurationsGrid : BaseComponent
    {
        [Inject] private IApplicationConfigurationService _appConfigService { get; set; }
        private IEnumerable<ApplicationConfigurationDTO> _pagedData;
        private MudTable<ApplicationConfigurationDTO> _table;
        private int _totalItems;
        private string _searchString = null;

        private async Task<TableData<ApplicationConfigurationDTO>> ServerReload(TableState state, CancellationToken cancellationToken) {
            IQueryable<ApplicationConfigurationDTO> data = _appConfigService.GetAll().Unwrap();
            await Task.Delay(300);
            if (!string.IsNullOrWhiteSpace(_searchString)) {
                data = data.Where(ac => ac.Id.Contains(_searchString));
            }

            _totalItems = data.Count();
            switch (state.SortLabel) {
                case nameof(ApplicationConfigurationDTO.Id):
                    data = data.OrderByDirection(state.SortDirection, o => o.Id);
                    break;
            }

            _pagedData = data.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new TableData<ApplicationConfigurationDTO>() { TotalItems = _totalItems, Items = _pagedData };
        }

        private async Task OnSearch(string text) {
            _searchString = text;
            await _table.ReloadServerData();
        }

        private async Task AddNew() {
            IDialogReference modalRef = ModalService.ShowModal<ApplicationConfigurationForm>("Add application configuration");
            Result result = await modalRef.GetReturnValueAsync<Result>();

            if (result != null && result.IsSuccessful) {
                await _table.ReloadServerData();
            }
        }

        private async Task Edit(string appConfigId) {
            ApplicationConfigurationDTO appConfig = (await _appConfigService.DetailsAsync(appConfigId, true)).Unwrap();
            var parameters = new Dictionary<string, object>
            {
                { nameof(ApplicationConfigurationForm.Model), appConfig }
            };
            IDialogReference modalRef = ModalService.ShowModal<ApplicationConfigurationForm>($"Editing application configuration '{appConfigId}'", parameters);
            Result result = await modalRef.GetReturnValueAsync<Result>();

            if (result != null && result.IsSuccessful) {
                await _table.ReloadServerData();
            }
        }

        private async Task Delete(string appConfigId) {
            bool confirmed = await ModalService.ShowConfirmationModalAsync(
                $"Delete application configuration '{appConfigId}'",
                $"Are you sure want to delete application configuration with id '{appConfigId}'?", "Delete", Color.Error);

            if (confirmed) {
                var deleteResult = await _appConfigService.DeleteAsync(appConfigId);
                if (deleteResult.IsSuccessful) {
                    ToastService.ShowSuccess($"App config with id '{appConfigId}' deleted successfully");
                }
                else {
                    ToastService.ShowError(deleteResult.MessageWithErrors);
                }

                if (deleteResult != null && deleteResult.IsSuccessful) {
                    await _table.ReloadServerData();
                }
            }
        }
    }
}
