﻿using System.Collections.Generic;
using System.Security.Claims;
using CH.CleanArchitecture.Common;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IIdentityContext
    {
        string UserId { get; }
        public string Username { get; }
        public string Name { get; }
        public string Culture { get; }
        public string UiCulture { get; }
        public IEnumerable<RoleEnum> Roles { get; }
        public ThemeEnum Theme { get; }
        public ClaimsPrincipal User { get; }

        void Initialize(ClaimsPrincipal user);
    }
}