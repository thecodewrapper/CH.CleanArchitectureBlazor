using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.Messaging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CH.CleanArchitecture.Presentation.Framework.Components
{
    public partial class ToggleComponent : BaseComponent
    {
        [Parameter]
        public bool State { get; set; }

        [Parameter]
        public string ToggleOnText { get; set; }

        [Parameter]
        public string ToggleOffText { get; set; }

        [Parameter]
        public IRequest<Result> ToggleOnCommand { get; set; }

        [Parameter]
        public IRequest<Result> ToggleOffCommand { get; set; }

        [Parameter]
        public Action<bool> OnResponse { get; set; }

        private async Task OnToggleOnClicked() {
            await SendCommand(ToggleOnCommand);
        }

        private async Task OnToggleOffClicked() {
            await SendCommand(ToggleOffCommand);
        }

        private async Task SendCommand(IRequest<Result> request) {
            Loader.Show();
            Result result;

            try {
                result = await SendRequestAsync(request, false);

            }
            catch (Exception ex) {
                LogExceptionError(ex, nameof(ToggleComponent));
                return;
            }

            Loader.Hide();
            if (result.IsSuccessful) {
                State = !State;
                ToastService.ShowSuccess(result.Message);
                OnResponse.Invoke(State);
            }
            else {
                ToastService.ShowError(result.MessageWithErrors);
            }
        }
    }
}
