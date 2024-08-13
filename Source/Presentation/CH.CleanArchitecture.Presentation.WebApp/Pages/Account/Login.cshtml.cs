// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.Messaging.Abstractions;
using CH.CleanArchitecture.Core.Application.Commands;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IServiceBus _serviceBus;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IServiceBus serviceBus, ILogger<LoginModel> logger) {
            _serviceBus = serviceBus;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null) {
            if (!string.IsNullOrEmpty(ErrorMessage)) {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid) {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var loginResult = await _serviceBus.SendAsync(new LoginUserCommand(Input.Email, Input.Password, Input.RememberMe));

                if (loginResult.IsFailed) {
                    ModelState.AddModelError(string.Empty, "Username or password is incorrect. Please try again.");
                    return Page();
                }

                var resultData = loginResult.Unwrap();
                if (resultData.Success) {
                    _logger.LogInformation($"User {resultData.User} logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (resultData.Requires2FA) {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                else {
                    ModelState.AddModelError(string.Empty, "Username or password is incorrect. Please try again.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
