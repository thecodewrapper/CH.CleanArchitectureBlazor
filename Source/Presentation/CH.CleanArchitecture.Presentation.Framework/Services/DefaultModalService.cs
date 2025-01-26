using System.Collections.Generic;
using System.Threading.Tasks;
using CH.CleanArchitecture.Presentation.Framework.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.Framework.Services
{
    public class DefaultModalService : Interfaces.IModalService
    {
        private readonly IDialogService _dialogService;

        public DefaultModalService(IDialogService dialogService) {
            _dialogService = dialogService;
        }

        public IDialogReference ShowModal<T>(string title, Dictionary<string, object> parameters) where T : ComponentBase {
            DialogParameters dialogParams = new DialogParameters();
            foreach (var param in parameters) {
                dialogParams.Add(param.Key, param.Value);
            }
            return _dialogService.Show<T>(title, dialogParams);
        }

        public IDialogReference ShowModal<T>(string title) where T : ComponentBase {
            return _dialogService.Show<T>(title);
        }

        public async Task<bool> ShowConfirmationModalAsync(string title, string contentText, string successButtonText, Color successButtonColor = default) {
            var parameters = new Dictionary<string, object>
            {
                { "ContentText", contentText },
                { "ButtonText", successButtonText },
                { "Color", successButtonColor }
            };
            var modalRef = ShowModal<ConfirmationDialog>(title, parameters);
            bool? returnValue = await modalRef.GetReturnValueAsync<bool?>();

            return returnValue ?? false;
        }

        public async Task<string> ShowConfirmationModalWithInputTextAsync(string title, string contentText, string successButtonText, string inputDescriptionText, Color successButtonColor = default) {
            var parameters = new Dictionary<string, object>
            {
                { "ContentText", contentText },
                { "ButtonText", successButtonText },
                { "Color", successButtonColor },
                { "InputDescriptionText", inputDescriptionText },
            };
            var modalRef = ShowModal<ConfirmationInputDialog>(title, parameters);
            string returnValue = await modalRef.GetReturnValueAsync<string>();

            return returnValue;
        }

        public async Task<bool> ShowConfirmationModalWithPhraseAsync(string title, string contentText, string successButtonText, string confirmationPhrase, Color successButtonColor = default) {
            var parameters = new Dictionary<string, object>
            {
                { "ContentText", contentText },
                { "ButtonText", successButtonText },
                { "Color", successButtonColor },
                { "ConfirmationPhrase", confirmationPhrase }
            };
            var modalRef = ShowModal<ConfirmationDialog>(title, parameters);
            bool? returnValue = await modalRef.GetReturnValueAsync<bool?>();

            return returnValue ?? false;
        }

        public void ShowInfoModal(string title, string contentText) {
            var parameters = new Dictionary<string, object>
            {
                { "ContentText", contentText },
                { "ButtonText", "OK" },
                { "ShowCancelButton", false },
            };
            ShowModal<ConfirmationDialog>(title, parameters);
        }
    }
}
