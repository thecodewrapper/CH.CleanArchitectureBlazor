using Microsoft.AspNetCore.Components;
using MudBlazor;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Models;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Developer
{
    public partial class ApplicationConfigurationForm : BaseComponent
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public ApplicationConfigurationDTO Model { get; set; }

        [Inject]
        private IApplicationConfigurationService _appConfigService { get; set; }

        private bool isNew;
        private ApplicationConfigurationFormModel _formModel = new ApplicationConfigurationFormModel();

        protected override void OnInitialized() {
            if (Model == null) {
                isNew = true;
                _formModel = new ApplicationConfigurationFormModel();
            }
            else {
                isNew = false;
                _formModel = Mapper.Map<ApplicationConfigurationFormModel>(Model);
            }
        }

        private async Task HandleValidSubmit() {
            Loader.Show();
            Result result;
            var dto = Mapper.Map<ApplicationConfigurationDTO>(_formModel);
            if (isNew) {
                result = await _appConfigService.CreateAsync(dto);
            }
            else {
                result = await _appConfigService.EditAsync(dto);
            }

            Loader.Hide();
            if (result.IsSuccessful) {
                ToastService.ShowSuccess(result.Message);
            }
            else {
                ToastService.ShowError(result.MessageWithErrors);
            }
            MudDialog.Close(DialogResult.Ok(result));
        }
    }
}
