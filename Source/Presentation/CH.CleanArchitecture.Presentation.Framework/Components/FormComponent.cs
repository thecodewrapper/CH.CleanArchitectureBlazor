using CH.Messaging.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using System;

namespace CH.CleanArchitecture.Presentation.Framework.Components
{
    public abstract class FormComponent<TFormModel, TReadModel, TCreateCommand, TUpdateCommand> : BaseComponent
        where TReadModel : IReadModel
        where TCreateCommand : ICommand, IRequest<Result>
        where TUpdateCommand : ICommand, IRequest<Result>
        where TFormModel : class, new()
    {
        [CascadingParameter]
        protected IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public TReadModel Model { get; set; }

        protected bool isNew;
        protected TFormModel _formModel = new TFormModel();

        protected override void OnInitialized() {
            if (Model == null) {
                isNew = true;
                _formModel = new TFormModel();
            }
            else {
                isNew = false;
                _formModel = Mapper.Map<TFormModel>(Model);
            }
        }

        protected async Task HandleValidSubmit() {
            Loader.Show();
            Result result;

            try {
                if (isNew) {
                    result = await SendRequestAsync(Mapper.Map<TCreateCommand>(_formModel));
                }
                else {
                    result = await SendRequestAsync(Mapper.Map<TUpdateCommand>(_formModel));
                }
            }
            catch (Exception ex) {
                LogExceptionError(ex, nameof(HandleValidSubmit));
                return;
            }

            Loader.Hide();
            if (result.IsSuccessful) {
                ToastService.ShowSuccess(result.Message);
                MudDialog.Close(DialogResult.Ok(result));
            }
            else {
                ToastService.ShowError(result.MessageWithErrors);
            }
        }
    }

    public abstract class FormComponent<TFormModel, TCreateCommand> : BaseComponent
    where TCreateCommand : ICommand, IRequest<Result>
    where TFormModel : class, new()
    {
        [CascadingParameter]
        protected IMudDialogInstance MudDialog { get; set; }

        protected TFormModel _formModel = new TFormModel();

        protected async Task HandleValidSubmit() {
            Loader.Show();
            Result result;

            try {
                result = await SendRequestAsync(Mapper.Map<TCreateCommand>(_formModel));
            }
            catch (Exception ex) {
                LogExceptionError(ex, nameof(HandleValidSubmit));
                return;
            }

            Loader.Hide();
            if (result.IsSuccessful) {
                ToastService.ShowSuccess(result.Message);
                MudDialog.Close(DialogResult.Ok(result));
            }
            else {
                ToastService.ShowError(result.MessageWithErrors);
            }
        }
    }

    public abstract class FormComponent<TFormModel, TCreateCommand, TCommandResult> : BaseComponent
    where TCreateCommand : ICommand, IRequest<Result<TCommandResult>>
    where TFormModel : class, new()
    {
        [CascadingParameter]
        protected IMudDialogInstance MudDialog { get; set; }

        protected TFormModel _formModel = new TFormModel();

        protected async Task HandleValidSubmit() {
            Loader.Show();
            Result result;
            try {
                result = await SendRequestAsync(Mapper.Map<TCreateCommand>(_formModel));
            }
            catch (Exception ex) {
                LogExceptionError(ex, nameof(HandleValidSubmit));
                return;
            }


            Loader.Hide();
            if (result.IsSuccessful) {
                ToastService.ShowSuccess(result.Message);
                MudDialog.Close(DialogResult.Ok(result));
            }
            else {
                ToastService.ShowError(result.MessageWithErrors);
            }
        }
    }
}
