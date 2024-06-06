﻿using System;
using System.Security.Claims;
using CH.CleanArchitecture.Core.Application.Constants;

namespace CH.CleanArchitecture.Core.Application
{
    public static class ClaimsPrincipalExtensions
    {
        public static string FindId(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.Id);
        }

        public static string FindFullName(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.FullName);
        }

        public static string FindUsername(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.Username);
        }

        public static string FindCulture(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.Culture);
        }

        public static string FindUiCulture(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.UiCulture);
        }

        public static string FindTheme(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.Theme);
        }

        public static string FindProfilePicture(this ClaimsPrincipal claimsPrincipal) {
            return claimsPrincipal?.FindFirstValue(ApplicationClaimTypes.User.ProfilePicture);
        }
    }
}
