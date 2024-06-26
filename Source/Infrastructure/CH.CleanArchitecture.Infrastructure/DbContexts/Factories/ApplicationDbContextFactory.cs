using System;
using CH.CleanArchitecture.Core.Application;
using Microsoft.EntityFrameworkCore;

namespace CH.CleanArchitecture.Infrastructure.DbContexts.Factories
{
    internal class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public ApplicationDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext, IAuthenticatedUserService authenticatedUserService) {
            _authenticatedUserService = authenticatedUserService;
            _configureDbContext = configureDbContext;
        }

        public ApplicationDbContext CreateDbContext() {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            _configureDbContext(optionsBuilder);
            return new ApplicationDbContext(optionsBuilder.Options, _authenticatedUserService);
        }
    }
}
