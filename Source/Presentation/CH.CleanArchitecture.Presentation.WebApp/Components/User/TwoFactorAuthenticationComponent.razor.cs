using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.Framework.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.User
{
    public partial class TwoFactorAuthenticationComponent : BaseComponent
    {
        [Inject] private IApplicationUserService _applicationUserService { get; set; }

        private bool _hasAuthenticator { get; set; }
        private bool _is2faEnabled { get; set; }
        private bool _isMachineRemembered { get; set; }
        private MudTabs _tabs;

        private bool _enableAuthenticator { get; set; } = true;
        private bool _canGenerateRecoveryCodes { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                Loader.Show();
                string userId = (await GetCurrentUserAsync()).FindId();
                _hasAuthenticator = (await _applicationUserService.GetAuthenticatorKeyAsync(userId)).Unwrap() != null;
                _is2faEnabled = (await _applicationUserService.GetTwoFactorAuthenticationStatusAsync(userId)).Unwrap();
                Loader.Hide();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task OnAuthenticatorEnabled() {
            _enableAuthenticator = false;
            _canGenerateRecoveryCodes = true;
            await StateHasChangedAsync();
        }

        private async Task ResetAuthenticatorKey() {
            Loader.Show();
            string userId = (await GetCurrentUserAsync()).FindId();
            var resetAuthenticatorResult = await _applicationUserService.ResetAuthenticatorAsync(userId);

            if (resetAuthenticatorResult.IsFailed) {
                ToastService.ShowError($"Unable to reset authenticator key");
                Loader.Hide();
                return;
            }

            ToastService.ShowSuccess("Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.");
            Loader.Hide();
            await StateHasChangedAsync();
        }

        private async Task Disable2FA() {
            Loader.Show();
            string userId = (await GetCurrentUserAsync()).FindId();
            var disable2faResult = await _applicationUserService.DisableTwoFactorAuthenticationAsync(userId);
            if (!disable2faResult.IsSuccessful) {
                ToastService.ShowError("Unexpected error occurred disabling 2FA.");
                Loader.Hide();
                return;
            }

            Logger.LogInformation("User with name '{UserId}' has disabled 2fa.", userId);
            ToastService.ShowSuccess("2fa has been disabled. You can reenable 2fa when you setup an authenticator app");
            _tabs.ActivatePanel(0, true);
            Loader.Hide();
            await StateHasChangedAsync();
        }
    }
}
