using AutoMapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.Framework.ViewModels;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Shared
{
    public partial class AppBar : BaseComponent
    {
        [Inject]
        private IMapper _mapper { get; set; }

        [CascadingParameter(Name = "ThemeProvider")]
        public MudThemeProvider ThemeProvider { get; set; }

        [Parameter]
        public EventCallback OnSidebarToggled { get; set; }

        [Parameter]
        public EventCallback OnDarkModeToggled { get; set; }

        private List<NotificationViewModel> _notifications = new();

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                var user = await GetCurrentUserAsync();
                var query = new GetAllNotificationsQuery()
                {
                    UserFor = user.FindId(),
                    Type = NotificationType.Portal,
                    Options = new QueryOptions()
                    {
                        OrderBy = "DateCreated",
                        IsAscending = false,
                        PageSize = 10,
                        PageNo = 1
                    }
                };
                var allNotificationsResult = await SendRequestAsync(query, false);

                if (allNotificationsResult.IsSuccessful) {
                    var notifications = allNotificationsResult.Unwrap();
                    _notifications = _mapper.Map<List<NotificationViewModel>>(notifications);
                    await StateHasChangedAsync();
                }
            }
        }
    }
}
