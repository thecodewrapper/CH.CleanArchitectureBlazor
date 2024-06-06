using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CH.CleanArchitecture.Infrastructure.DbContexts;
using CH.CleanArchitecture.Infrastructure.Models;

namespace CH.CleanArchitecture.Infrastructure.EntityTypeConfigurations
{
    internal class ResourceEntityTypeConfiguration : IEntityTypeConfiguration<ResourceEntity>
    {
        public ResourceEntityTypeConfiguration() {
        }

        public void Configure(EntityTypeBuilder<ResourceEntity> builder) {
            builder.ToTable("Resources", ApplicationDbContext.APP_SCHEMA);
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
