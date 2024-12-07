using Microsoft.AspNetCore.Components;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Resources;
using CH.CleanArchitecture.Presentation.Framework;

namespace CH.CleanArchitecture.Presentation.WebApp.Services
{
    public class ToastService : IToastService
    {
        #region Private Members

        private readonly Blazored.Toast.Services.IToastService _blazoredService;
        private readonly ILocalizationService _localizer;

        #endregion

        #region Constructor

        public ToastService(ILocalizationService localizer,
            Blazored.Toast.Services.IToastService blazoredService) {
            _localizer = localizer;
            _blazoredService = blazoredService;
        }

        #endregion

        #region Public Methods

        public void ShowSuccess(string message) {
            _blazoredService.ShowSuccess(message);
        }

        public void ShowError(string message) {
            _blazoredService.ShowError(message ?? _localizer[ResourceKeys.Common_SomethingWentWrong]);
        }

        public void ShowError(RenderFragment message) {
            _blazoredService.ShowError(message);
        }

        public void ShowWarning(string message) {
            _blazoredService.ShowWarning(message);
        }

        public void ShowInfo(string message) {
            _blazoredService.ShowInfo(message);
        }

        #endregion
    }
}
