using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Presentation.Framework.Interfaces
{
    public interface IModalService
    {
        /// <summary>
        /// Shows the modal and returns an <see cref="object"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDialogReference ShowModal<T>(string title, Dictionary<string, object> parameters) where T : ComponentBase;

        /// <summary>
        /// Shows the modal and returns an <see cref="object"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <returns></returns>
        IDialogReference ShowModal<T>(string title) where T : ComponentBase;

        /// <summary>
        /// Shows a confirmation modal
        /// </summary>
        /// <param name="title"></param>
        /// <param name="contentText"></param>
        /// <param name="successButtonText"></param>
        /// <param name="successButtonColor"></param>
        /// <returns></returns>
        Task<bool> ShowConfirmationModalAsync(string title, string contentText, string successButtonText, Color successButtonColor = default);

        /// <summary>
        /// Shows a confirmation modal with a text input
        /// </summary>
        /// <param name="title"></param>
        /// <param name="contentText"></param>
        /// <param name="successButtonText"></param>
        /// <param name="inputDescriptionText"></param>
        /// <param name="successButtonColor"></param>
        /// <returns></returns>
        Task<string> ShowConfirmationModalWithInputTextAsync(string title, string contentText, string successButtonText, string inputDescriptionText, Color successButtonColor = default);

        /// <summary>
        /// Shows a confirmation model, along with a phrase that needs to be typed before you can submit
        /// </summary>
        /// <param name="title"></param>
        /// <param name="contentText"></param>
        /// <param name="successButtonText"></param>
        /// <param name="confirmationPhrase"></param>
        /// <param name="successButtonColor"></param>
        /// <returns></returns>
        Task<bool> ShowConfirmationModalWithPhraseAsync(string title, string contentText, string successButtonText, string confirmationPhrase, Color successButtonColor = default);

        /// <summary>
        /// Shows an information modal with an OK button. Used for notifying user.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="contentText"></param>
        void ShowInfoModal(string title, string contentText);
    }
}
