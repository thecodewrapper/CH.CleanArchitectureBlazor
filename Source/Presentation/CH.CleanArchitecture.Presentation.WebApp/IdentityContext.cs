﻿using System.Security.Claims;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Presentation.WebApp
{
    public class IdentityContext : IIdentityContext
    {
        private const string FALLBACK_USERNAME = "SystemUser";
        private readonly ILogger<IdentityContext> _logger;

        public string UserId { get; private set; }
        public string Username { get; private set; }
        public string Name { get; private set; }
        public string ProfilePicture { get; private set; }
        public string Culture { get; private set; }
        public string UiCulture { get; private set; }
        public IEnumerable<RoleEnum> Roles { get; private set; }
        public ThemeEnum Theme { get; private set; }
        public ClaimsPrincipal User { get; private set; }

        public IdentityContext(ILogger<IdentityContext> logger) {
            _logger = logger;
            Username = FALLBACK_USERNAME;
        }

        public void Initialize(ClaimsPrincipal user) {
            if (user == null) {
                return;
            }

            User = user;
            UserId = user.FindId() ?? null;
            Username = user.FindFullName() ?? null;
            Name = user.FindFullName() ?? null;
            Culture = user.FindCulture() ?? null;
            UiCulture = user.FindUiCulture() ?? null;
            Theme = user.FindTheme()?.ToEnum<ThemeEnum>() ?? default;

            var profilePictureClaim = user.FindProfilePicture();
            if (profilePictureClaim != null) {
                ProfilePicture = profilePictureClaim;
            }

            var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role);
            if (roles != null)
                Roles = roles.Select(r => r.Value.ToEnum<RoleEnum>());
        }
    }
}