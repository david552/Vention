using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vention.Domain.Organizations;

namespace Vention.Infrastructure.Persistence.Configurations
{
    public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("organizations");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new OrganizationId(value))
                .ValueGeneratedNever();

            builder.Property(o => o.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(o => o.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(o => o.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamptz");

            builder.HasQueryFilter(o => !o.IsDeleted);

        }
    }
}
