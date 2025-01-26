using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.Queries;
using CH.CleanArchitecture.Core.Application.ReadModels;
using CH.CleanArchitecture.Presentation.Framework.Components;
using CH.CleanArchitecture.Presentation.WebApp.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace CH.CleanArchitecture.Presentation.WebApp.Components.User
{
    public partial class UserDetailsForm : BaseComponent
    {
        private UserDetailsFormModel model = new UserDetailsFormModel();

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                await LoadUserProfile();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task OnValidSubmit(EditContext context) {
            var user = await GetCurrentUserAsync();
            var command = new UpdateUserDetailsCommand(user.FindId())
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
            var user = await GetCurrentUserAsync();
            Result<UserReadModel> userQuery = await SendRequestAsync(new GetUserQuery()
            {
                Id = user.FindId()
            });

            if (userQuery.IsFailed) {
                NavigationManager.NavigateTo("/error");
            }

            model.Email = userQuery.Data.Email;
            model.PhoneNumber = userQuery.Data.PhoneNumber;
            model.SecondaryPhoneNumber = userQuery.Data.SecondaryPhoneNumber;
            model.Name = userQuery.Data.Name;
            model.Surname = userQuery.Data.Surname;
            model.LanguagePreference = userQuery.Data.UICulture;

            AddressReadModel addressReadModel = userQuery.Data.Address ?? new AddressReadModel();
            model.Address = Mapper.Map<AddressDTO>(addressReadModel);

            await StateHasChangedAsync();
        }
    }
}
