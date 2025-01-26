// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.WebApp.Extensions;
using CH.CleanArchitecture.Presentation.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly UserAccountService _userAccountService;

        public ForgotPasswordModel(IApplicationUserService applicationUserService, UserAccountService userAccountService) {
            _applicationUserService = applicationUserService;
            _userAccountService = userAccountService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync() {
            if (ModelState.IsValid) {
                string baseUri = HttpContext.GetBaseUri();
                await _userAccountService.SendResetPasswordEmailAsync(Input.Email, baseUri);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
