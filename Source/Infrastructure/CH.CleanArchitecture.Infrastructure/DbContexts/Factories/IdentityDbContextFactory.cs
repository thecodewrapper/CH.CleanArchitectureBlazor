using System;
using CH.CleanArchitecture.Core.Application;
using Microsoft.EntityFrameworkCore;

namespace CH.CleanArchitecture.Infrastructure.DbContexts.Factories
{
    internal class IdentityDbContextFactory : IDbContextFactory<IdentityDbContext>
    {
        private readonly IIdentityContext _identityContext;
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public IdentityDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext, IIdentityContext identityContext) {
            _identityContext = identityContext;
            _configureDbContext = configureDbContext;
        }

        public IdentityDbContext CreateDbContext() {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
            _configureDbContext(optionsBuilder);
            return new IdentityDbContext(optionsBuilder.Options, _identityContext);
        }
    }
}
