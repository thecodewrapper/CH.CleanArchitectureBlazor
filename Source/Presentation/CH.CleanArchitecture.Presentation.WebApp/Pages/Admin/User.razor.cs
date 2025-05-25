using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Models;
using CH.CleanArchitecture.Presentation.WebApp.Services;
using CH.CleanArchitecture.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Admin
{
    public partial class User : BaseComponent
    {
        private UserDetailsFormModel model = new UserDetailsFormModel();
        private UserReadModel _userReadModel = new();

        private MudSelect<string> _rolesMultiSelect;
        private List<string> _selectedRoles;

        [Inject]
        private UserAccountService UserAccountService { get; set; }

        [Parameter]
        public string Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                await LoadUserProfile();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task OnValidSubmit(EditContext context) {
            var command = new UpdateUserDetailsCommand(Id)
            {
                //Email = model.Email,
                PrimaryPhone = model.PhoneNumber,
                SecondaryPhone = model.SecondaryPhoneNumber,
                Name = model.Name,
                Surname = model.Surname,
                Address = model.Address,
                LanguagePreference = model.LanguagePreference
            };
            Result result = await SendRequestAsync(command);

            if (result.IsFailed) {
                ToastService.ShowError("Unable to update user profile");
            }
            else {
                ToastService.ShowSuccess("Profile details updated successfully");
            }

            await StateHasChangedAsync();
        }

        private async Task LoadUserProfile() {
            Result<UserReadModel> userQuery = await SendRequestAsync(new GetUserQuery()
            {
                Id = Id
            });

            if (userQuery.IsFailed) {
                NavigationManager.NavigateTo("/error");
            }

            _userReadModel = userQuery.Data;

            model.Email = _userReadModel.Email;
            model.PhoneNumber = _userReadModel.PhoneNumber;
            model.SecondaryPhoneNumber = _userReadModel.SecondaryPhoneNumber;
            model.Name = _userReadModel.Name;
            model.Surname = _userReadModel.Surname;
            model.LanguagePreference = _userReadModel.UICulture;

            _selectedRoles = _userReadModel.Roles.Select(r => r.ToString()).ToList();

            AddressReadModel addressReadModel = _userReadModel.Address ?? new AddressReadModel();
            model.Address = Mapper.Map<AddressDTO>(addressReadModel);

            await StateHasChangedAsync();
        }

        private async Task Activate() {
            bool confirmApproval = await ModalService.ShowConfirmationModalAsync(string.Format(L.GetLocalizedString(ResourceKeys.Messages_ActivatingUser), Id), L.GetLocalizedString(ResourceKeys.Messages_ActivateUserConfirmation), L.GetLocalizedString(ResourceKeys.Buttons_Activate_Cap), Color.Success);
            if (confirmApproval) {
                await SendRequestAsync(new ActivateUserCommand(_userReadModel.Username), string.Format(L.GetLocalizedString(ResourceKeys.Messages_UnableToActivateUser), Id), string.Format(L.GetLocalizedString(ResourceKeys.Messages_UserActivatedSuccessfully), Id));
                await LoadUserProfile();
            }
        }

        private async Task Deactivate() {
            bool confirmRejection = await ModalService.ShowConfirmationModalAsync(
                    string.Format(L.GetLocalizedString(ResourceKeys.Messages_DeactivatingUser), Id),
                    L.GetLocalizedString(ResourceKeys.Messages_DeactivateUserConfirmation),
                    L.GetLocalizedString(ResourceKeys.Buttons_Deactivate_Cap),
                    Color.Error
                );

            if (confirmRejection) {
                await SendRequestAsync(
                            new DeactivateUserCommand(_userReadModel.Username),
                            string.Format(L.GetLocalizedString(ResourceKeys.Messages_UnableToDeactivateUser), Id),
                            string.Format(L.GetLocalizedString(ResourceKeys.Messages_UserDeactivatedSuccessfully), Id)
                        );
                await LoadUserProfile();
            }
        }

        private async Task ResendConfirmationEmail() {
            bool confirm = await ModalService.ShowConfirmationModalAsync(
                L.GetLocalizedString(ResourceKeys.Messages_ResendConfirmationEmail),
                string.Format(L.GetLocalizedString(ResourceKeys.Messages_ResendConfirmationEmailConfirmation), _userReadModel.Email),
                L.GetLocalizedString(ResourceKeys.Buttons_YesSendIt)
            );
            if (confirm) {
                await UserAccountService.SendConfirmationEmailAsync(_userReadModel.Email, NavigationManager.BaseUri);
            }
        }

        private async Task ConfirmUserEmail() {
            bool confirm = await ModalService.ShowConfirmationModalAsync(
                L.GetLocalizedString(ResourceKeys.Messages_ConfirmUserEmail),
                string.Format(L.GetLocalizedString(ResourceKeys.Messages_ConfirmUserEmailConfirmation), _userReadModel.Email),
                L.GetLocalizedString(ResourceKeys.Common_Yes)
            );
            if (confirm) {
                await SendRequestAsync(
                    new ConfirmUserEmailCommand(_userReadModel.Id),
                    string.Format(L.GetLocalizedString(ResourceKeys.Messages_UnableToConfirmUserEmail), _userReadModel.Id),
                    string.Format(L.GetLocalizedString(ResourceKeys.Messages_UserEmailConfirmedSuccessfully), _userReadModel.Email)
                );
                await LoadUserProfile();
            }
        }

        private async Task UpdateRoles() {
            if (!_selectedRoles.Any()) {
                ToastService.ShowError($"You have to select at least one role");
                return;
            }

            var result = await SendRequestAsync(new UpdateUserRolesCommand()
            {
                Roles = _selectedRoles,
                Username = _userReadModel.Username
            });

            if (result.IsSuccessful) {
                await LoadUserProfile();
            }
        }

        private void SelectedRolesChanged(IEnumerable<string> values) {
            _selectedRoles = values.ToList();
            _rolesMultiSelect.ResetValidation();
            if (!values.Any()) {
                _rolesMultiSelect.Validate();
            }
            StateHasChanged();
        }
    }
}
