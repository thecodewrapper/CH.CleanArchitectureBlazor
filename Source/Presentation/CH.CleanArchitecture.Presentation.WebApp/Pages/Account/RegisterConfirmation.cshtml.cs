// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly IApplicationUserService _applicationUserService;

        public RegisterConfirmationModel(IApplicationUserService applicationUserService) {
            _applicationUserService = applicationUserService;
        }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null) {
            if (email == null) {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");
            var user = (await _applicationUserService.GetUserByEmailAsync(email)).Unwrap();
            if (user == null) {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            return Page();
        }
    }
}
