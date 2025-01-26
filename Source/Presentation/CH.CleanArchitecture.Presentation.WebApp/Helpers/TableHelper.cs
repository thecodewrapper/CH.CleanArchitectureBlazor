using CH.CleanArchitecture.Common;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.WebApp.Helpers
{
    internal static class TableHelper
    {
        internal static QueryOptions GetQueryOptionsFromTableState(TableState state, string searchTerm = default) {
            var queryOptions = new QueryOptions()
            {
                IsAscending = state.SortDirection == SortDirection.Ascending,
                OrderBy = state.SortLabel,
                PageNo = state.Page,
                PageSize = state.PageSize,
                Skip = state.Page * state.PageSize,
                SearchTerm = searchTerm
            };
            if (!string.IsNullOrWhiteSpace(searchTerm)) {
                queryOptions.SearchTerm = searchTerm;
            }

            return queryOptions;
        }
    }
}
