using System;
using CH.CleanArchitecture.Core.Application;
using Microsoft.EntityFrameworkCore;

namespace CH.CleanArchitecture.Infrastructure.DbContexts.Factories
{
    internal class IdentityDbContextFactory : IDbContextFactory<IdentityDbContext>
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public IdentityDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext, IAuthenticatedUserService authenticatedUserService) {
            _authenticatedUserService = authenticatedUserService;
            _configureDbContext = configureDbContext;
        }

        public IdentityDbContext CreateDbContext() {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
            _configureDbContext(optionsBuilder);
            return new IdentityDbContext(optionsBuilder.Options, _authenticatedUserService);
        }
    }
}
