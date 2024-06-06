// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.WebApp.Extensions;
using CH.CleanArchitecture.Presentation.WebApp.Services;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly UserAccountService _userAccountService;

        public ResendEmailConfirmationModel(IApplicationUserService applicationUserService, UserAccountService userAccountService) {
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

        public void OnGet() {
        }

        public async Task<IActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                return Page();
            }
            string baseUri = HttpContext.GetBaseUri();
            await _userAccountService.SendConfirmationEmailAsync(Input.Email, baseUri);

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }
    }
}
