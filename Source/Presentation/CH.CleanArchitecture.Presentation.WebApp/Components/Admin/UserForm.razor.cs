using MudBlazor;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Models;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.Admin
{
    public partial class UserForm : FormComponent<UserFormModel, CreateUserCommand>
    {
        MudSelect<string> _rolesMultiSelect;

        private void SelectedRolesChanged(IEnumerable<string> values) {
            _formModel.Roles = values.ToList();
            _rolesMultiSelect.ResetValidation();
            if (!values.Any()) {
                _rolesMultiSelect.Validate();
            }
            StateHasChanged();
        }
    }
}
