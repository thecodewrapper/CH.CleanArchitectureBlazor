using System.Runtime.CompilerServices;
using CH.CleanArchitecture.Core.Application;
using CH.CleanArchitecture.Infrastructure.EntityTypeConfigurations;
using CH.CleanArchitecture.Infrastructure.Models;
using CH.Data.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("CH.CleanArchitecture.Tests")]
namespace CH.CleanArchitecture.Infrastructure.DbContexts
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>,
        ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>,
        IUnitOfWork
    {
        private const string SCHEMA = "Identity";
        private readonly IAuthenticatedUserService _authenticatedUser;

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) {
        }

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options, IAuthenticatedUserService authenticatedUser) : base(options) {
            _authenticatedUser = authenticatedUser;
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema(SCHEMA);
            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new ApplicationRoleConfiguration());
            builder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
            builder.ApplyConfiguration(new UserClaimsConfiguration());
            builder.ApplyConfiguration(new RoleClaimsConfiguration());
            builder.ApplyConfiguration(new UserLoginsConfiguration());
            builder.ApplyConfiguration(new UserTokensConfiguration());
            builder.ApplyConfiguration(new AddressEntityConfiguration("UserAddresses", SCHEMA));
        }
    }
}