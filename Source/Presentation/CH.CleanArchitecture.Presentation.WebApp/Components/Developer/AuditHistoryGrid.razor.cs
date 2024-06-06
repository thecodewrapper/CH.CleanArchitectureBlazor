using Microsoft.AspNetCore.Components;
using MudBlazor;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Presentation.Framework.Components;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Developer
{
    public partial class AuditHistoryGrid : BaseComponent
    {
        [Inject] private IAuditHistoryService _auditHistoryService { get; set; }

        private IEnumerable<AuditHistoryDTO> pagedData;
        private MudTable<AuditHistoryDTO> table;
        private int totalItems;
        private string searchString = null;
        private AuditHistoryDTO _selectedItem;

        /// <summary>
        /// Here we simulate getting the paged, filtered and ordered data from the server
        /// </summary>
        private async Task<TableData<AuditHistoryDTO>> ServerReload(TableState state) {
            IQueryable<AuditHistoryDTO> data = _auditHistoryService.GetAll().Unwrap();
            await Task.Delay(300);
            if (!string.IsNullOrWhiteSpace(searchString)) {
                data = data.Where(ah => ah.TableName.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            totalItems = data.Count();
            switch (state.SortLabel) {
                case nameof(AuditHistoryDTO.TableName):
                    data = data.OrderByDirection(state.SortDirection, o => o.TableName);
                    break;
                case nameof(AuditHistoryDTO.Kind):
                    data = data.OrderByDirection(state.SortDirection, o => o.Kind);
                    break;
                case nameof(AuditHistoryDTO.Created):
                    data = data.OrderByDirection(state.SortDirection, o => o.Created);
                    break;
                case nameof(AuditHistoryDTO.Username):
                    data = data.OrderByDirection(state.SortDirection, o => o.Username);
                    break;
            }

            pagedData = data.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new TableData<AuditHistoryDTO>() { TotalItems = totalItems, Items = pagedData };
        }

        private void OnSearch(string text) {
            searchString = text;
            table.ReloadServerData();
        }

        private async Task ShowAuditHistoryDetails(int auditHistoryId) {
            _selectedItem = (await _auditHistoryService.Details(auditHistoryId)).Unwrap();
        }
    }
}
