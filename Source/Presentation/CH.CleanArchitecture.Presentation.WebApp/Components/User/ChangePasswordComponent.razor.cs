using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.User
{
    public partial class ChangePasswordComponent : BaseComponent
    {
        private ChangePasswordFormModel model = new();

        private async Task OnValidSubmit(EditContext context) {
            var user = await GetCurrentUserAsync();
            var result = await SendRequestAsync(new ChangeUserPasswordCommand(user.FindId(), model.OldPassword, model.Password));
            if (result.IsFailed) {
                ToastService.ShowError(result.MessageWithErrors);
            }
            else {
                ToastService.ShowSuccess("Password changed successfully");
            }
            await StateHasChangedAsync();
        }
    }
}
