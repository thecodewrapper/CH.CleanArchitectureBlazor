// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly ILogger<ConfirmEmailModel> _logger;
        private readonly IApplicationUserService _applicationUserService;
        private readonly UserAccountService _userAccountService;

        public ConfirmEmailModel(ILogger<ConfirmEmailModel> logger, IApplicationUserService applicationUserService, UserAccountService userAccountService) {
            _logger = logger;
            _applicationUserService = applicationUserService;
            _userAccountService = userAccountService;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code) {
            if (userId == null || code == null) {
                return RedirectToPage("./Login");
            }

            _logger.LogInformation($"Confirming email for user '{userId}'. Code: {code}");
            string decodedCode = _userAccountService.DecodeEmailToken(code);
            var result = await _applicationUserService.ConfirmUserEmailAsync(userId, decodedCode);

            StatusMessage = result.IsSuccessful ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }
    }
}
