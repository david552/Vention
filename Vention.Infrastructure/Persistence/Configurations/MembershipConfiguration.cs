using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Configurations
{
    public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.ToTable("memberships");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new MembershipId(value))
                .ValueGeneratedNever();

            builder.Property(m => m.UserId)
                .HasColumnName("user_id")
                .HasConversion(id => id.Value, value => new UserId(value))
                .IsRequired();

            builder.Property(m => m.OrganizationId)
                .HasColumnName("organization_id")
                .HasConversion(id => id.Value, value => new OrganizationId(value))
                .IsRequired();

            builder.Property(m => m.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(m => m.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(m => m.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => new { m.UserId, m.OrganizationId }).IsUnique();
        }
    }
}
