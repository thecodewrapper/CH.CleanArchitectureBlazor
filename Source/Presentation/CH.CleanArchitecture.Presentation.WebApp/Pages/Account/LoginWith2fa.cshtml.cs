// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.Messaging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly ILogger<LoginWith2faModel> _logger;
        private readonly IServiceBus _serviceBus;
        private readonly IUserAuthenticationService _userAuthenticationService;

        public LoginWith2faModel(ILogger<LoginWith2faModel> logger, IServiceBus serviceBus, IUserAuthenticationService userAuthenticationService) {
            _logger = logger;
            _serviceBus = serviceBus;
            _userAuthenticationService = userAuthenticationService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null) {
            // Ensure the user has gone through the username & password screen first
            var userResult = await _userAuthenticationService.GetTwoFactorAuthenticationUserAsync();
            if (userResult.IsFailed) {
                return RedirectToPage("./Login");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null) {
            if (!ModelState.IsValid) {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            string authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var loginResult = await _serviceBus.SendAsync(new LoginUser2FACommand(authenticatorCode, false));

            if (loginResult.IsFailed) {
                return RedirectToPage("/Error");
            }

            var resultData = loginResult.Unwrap();
            if (resultData.Success) {
                _logger.LogInformation("User '{User}' logged in with 2fa.", resultData.User);
                return LocalRedirect(returnUrl);
            }
            else if (resultData.IsLockedOut) {
                _logger.LogWarning("User '{User}' account locked out.", resultData.User);
                return RedirectToPage("./Login");
            }
            else {
                _logger.LogWarning("Invalid authenticator code entered for user '{User}'.", resultData.User);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}
