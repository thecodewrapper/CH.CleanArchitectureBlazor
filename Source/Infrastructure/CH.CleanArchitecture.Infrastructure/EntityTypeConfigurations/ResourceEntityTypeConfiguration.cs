using CH.CleanArchitecture.Infrastructure.DbContexts;
using CH.CleanArchitecture.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CH.CleanArchitecture.Infrastructure.EntityTypeConfigurations
{
    internal class ResourceEntityTypeConfiguration : IEntityTypeConfiguration<ResourceEntity>
    {
        public ResourceEntityTypeConfiguration() {
        }

        public void Configure(EntityTypeBuilder<ResourceEntity> builder) {
            builder.ToTable("Resources", ApplicationDbContext.APP_SCHEMA);
            builder.HasKey(a => a.Id);

            builder.Property(r => r.Extension).HasMaxLength(16);
            builder.Property(r => r.Meta).HasMaxLength(1024);
            builder.Property(r => r.Name).HasMaxLength(256);
            builder.Property(r => r.ContainerName).HasMaxLength(256);
            builder.Property(r => r.URI).HasMaxLength(1024);
            builder.Property(r => r.Domain).HasMaxLength(256).IsRequired(false);

            builder.Property(r => r.CreatedBy).HasMaxLength(64);
            builder.Property(r => r.ModifiedBy).HasMaxLength(64);
        }
    }
}
