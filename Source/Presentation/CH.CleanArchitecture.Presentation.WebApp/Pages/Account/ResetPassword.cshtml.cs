// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.WebApp.Services;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly UserAccountService _userAccountService;

        public ResetPasswordModel(IApplicationUserService applicationUserService, UserAccountService userAccountService) {
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

            [Required]
            [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Code { get; set; }

        }

        public IActionResult OnGet(string code = null) {
            if (code == null) {
                return BadRequest("A code must be supplied for password reset.");
            }
            else {
                Input = new InputModel
                {
                    Code = _userAccountService.DecodeEmailToken(code)
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                return Page();
            }

            var result = await _applicationUserService.ResetPasswordAsync(Input.Email, Input.Code, Input.Password);
            if (result.IsFailed) {
                ModelState.AddModelError(string.Empty, result.MessageWithErrors);
                return Page();
            }

            return RedirectToPage("./ResetPasswordConfirmation");
        }
    }
}
