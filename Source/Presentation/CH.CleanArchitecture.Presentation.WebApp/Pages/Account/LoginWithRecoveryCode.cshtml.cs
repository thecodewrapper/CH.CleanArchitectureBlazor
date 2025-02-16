// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    public class LoginWithRecoveryCodeModel : PageModel
    {
        private readonly ILogger<LoginWithRecoveryCodeModel> _logger;
        private readonly IServiceBusMediator _serviceBus;
        private readonly IUserAuthenticationService _userAuthenticationService;

        public LoginWithRecoveryCodeModel(ILogger<LoginWithRecoveryCodeModel> logger, IServiceBusMediator serviceBus, IUserAuthenticationService userAuthenticationService) {
            _logger = logger;
            _serviceBus = serviceBus;
            _userAuthenticationService = userAuthenticationService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [BindProperty]
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
            public string RecoveryCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null) {
            // Ensure the user has gone through the username & password screen first
            var userResult = await _userAuthenticationService.GetTwoFactorAuthenticationUserAsync();
            if (userResult.IsFailed) {
                return RedirectToPage("./Login");
            }

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            if (!ModelState.IsValid) {
                return Page();
            }

            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);
            var loginResult = await _serviceBus.SendAsync(new LoginUser2FACommand(recoveryCode, true));

            if (loginResult.IsFailed) {
                return RedirectToPage("/Error");
            }

            var resultData = loginResult.Unwrap();
            if (resultData.Success) {
                _logger.LogInformation("User '{User}' logged in with a recovery code.", resultData.User);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            if (resultData.IsLockedOut) {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Login");
            }
            else {
                _logger.LogWarning("Invalid recovery code entered for user '{User}' ", resultData.User);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return Page();
            }
        }
    }
}
