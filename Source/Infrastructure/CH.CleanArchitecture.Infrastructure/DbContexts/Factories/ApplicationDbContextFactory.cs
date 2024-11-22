using System;
using CH.CleanArchitecture.Core.Application;
using Microsoft.EntityFrameworkCore;

namespace CH.CleanArchitecture.Infrastructure.DbContexts.Factories
{
    internal class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly IIdentityContext _identityContext;
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public ApplicationDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext, IIdentityContext identityContext) {
            _identityContext = identityContext;
            _configureDbContext = configureDbContext;
        }

        public ApplicationDbContext CreateDbContext() {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            _configureDbContext(optionsBuilder);
            return new ApplicationDbContext(optionsBuilder.Options, _identityContext);
        }
    }
}
