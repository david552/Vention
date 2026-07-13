using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new UserId(value))
                .ValueGeneratedNever();

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("email")
                    .HasMaxLength(320)
                    .IsRequired();

                email.HasIndex(e => e.Value)
                .IsUnique()
                .HasFilter("\"is_deleted\" = false"); 
            });

            builder.Property<long>("Sequence")
                .HasColumnName("sequence")
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn();

            builder.Property(u => u.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(u => u.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamptz");

            builder.Property(u => u.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasQueryFilter(u => !u.IsDeleted);

        }
    }
}
