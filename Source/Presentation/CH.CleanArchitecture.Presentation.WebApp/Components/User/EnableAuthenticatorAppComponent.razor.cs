using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.User
{
    public partial class EnableAuthenticatorAppComponent : BaseComponent
    {
        [Inject] private IJSRuntime _jsRuntime { get; set; }
        [Inject] private IApplicationUserService _applicationUserService { get; set; }

        [Parameter] public EventCallback OnAuthenticatorEnabled { get; set; }

        private TwoFactorVerificationFormModel model = new();
        private string _sharedKey { get; set; }
        private string _authenticatorUri { get; set; }

        private string _userLanguage = "en";
        protected override async Task OnInitializedAsync() {
            var user = await GetCurrentUserAsync();
            string userId = user.FindId();
            _userLanguage = user.FindCulture();
            await LoadSharedKeyAndQrCodeUriAsync(userId);
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                await _jsRuntime.InvokeVoidAsync("generateQrCode");
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(string userId) {
            (_sharedKey, _authenticatorUri) = (await _applicationUserService.GetAuthenticatorSharedKeyAndQrCodeUriAsync(userId)).Unwrap();
        }

        private async Task OnValidSubmit(EditContext context) {
            Loader.Show();
            string userId = (await GetCurrentUserAsync()).FindId();
            // Strip spaces and hyphens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var enable2faResult = await _applicationUserService.EnableAuthenticatorAsync(userId, verificationCode);

            if (enable2faResult.IsFailed) {
                ToastService.ShowError(enable2faResult.MessageWithErrors);
                await LoadSharedKeyAndQrCodeUriAsync(userId);
                await StateHasChangedAsync();
                Loader.Hide();
                return;
            }

            ToastService.ShowSuccess("Your authenticator app has been verified.");
            Loader.Hide();
            await StateHasChangedAsync();

            await OnAuthenticatorEnabled.InvokeAsync();
        }
    }
}
