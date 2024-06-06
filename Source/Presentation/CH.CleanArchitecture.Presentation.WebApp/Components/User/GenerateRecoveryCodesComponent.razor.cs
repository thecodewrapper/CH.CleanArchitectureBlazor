using Microsoft.AspNetCore.Components;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Common.Extensions;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.Framework.Components;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.User
{
    public partial class GenerateRecoveryCodesComponent : BaseComponent
    {
        [Inject] private IApplicationUserService _applicationUserService { get; set; }

        private List<string> _recoveryCodes { get; set; } = new();
        private bool _twoFactorEnabled { get; set; } = false;

        protected override async Task OnInitializedAsync() {
            var user = await GetCurrentUserAsync();
            string userId = user.FindId();
            var twoFactorResult = await _applicationUserService.GetTwoFactorAuthenticationStatusAsync(userId);
            if (twoFactorResult.IsSuccessful) {
                _twoFactorEnabled = twoFactorResult.Unwrap();
            }
            else {
                ToastService.ShowError(twoFactorResult.MessageWithErrors);
            }
            await base.OnInitializedAsync();
        }

        private async Task GenerateRecoveryCodes() {
            var user = await GetCurrentUserAsync();
            string userId = user.FindId();

            var recoveryCodes = await _applicationUserService.GenerateTwoFactorRecoveryCodesAsync(userId, 10);
            _recoveryCodes = recoveryCodes.Unwrap().ToList();

            Logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);
            ToastService.ShowSuccess("You have generated new recovery codes.");
        }
    }
}
