using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Models;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Admin
{
    public partial class UserForm : FormComponent<UserFormModel, CreateUserCommand>
    {
        MudSelect<string> _rolesMultiSelect;

        private async Task SelectedRolesChanged(IEnumerable<string> values) {
            _formModel.Roles = values.ToList();
            await _rolesMultiSelect.ResetValidationAsync();
            if (!values.Any()) {
                await _rolesMultiSelect.ValidateAsync();
            }
            StateHasChanged();
        }
    }
}
