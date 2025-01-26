// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Presentation.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    public class LogoutModel : AccountPageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly IUserAuthenticationService _userAuthenticationService;

        public LogoutModel(ILogger<LogoutModel> logger, IUserAuthenticationService userAuthenticationService) {
            _logger = logger;
            _userAuthenticationService = userAuthenticationService;
        }

        public async Task<IActionResult> OnPost() {
            await _userAuthenticationService.Logout(User);
            _logger.LogInformation("User logged out.");
            return Redirect(Url.Content("~/"));
        }
    }
}
