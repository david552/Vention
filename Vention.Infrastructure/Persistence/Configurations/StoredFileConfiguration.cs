using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vention.Domain.Files;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Configurations
{
    public sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
    {
        public void Configure(EntityTypeBuilder<StoredFile> builder)
        {
            builder.ToTable("files");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new StoredFileId(value))
                .ValueGeneratedNever();

            builder.Property(f => f.Filename)
                .HasColumnName("filename")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(f => f.Size)
                .HasColumnName("size")
                .IsRequired();

            builder.Property(f => f.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(f => f.ContentType)
                .HasColumnName("content_type")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(f => f.Checksum)
                .HasColumnName("checksum")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(f => f.StorageKey)
                .HasColumnName("storage_key")
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(f => f.OrganizationId)
                .HasColumnName("organization_id")
                .HasConversion(id => id.Value, value => new OrganizationId(value))
                .IsRequired();

            builder.Property(f => f.OwnerId)
                .HasColumnName("owner_id")
                .HasConversion(id => id.Value, value => new UserId(value))
                .IsRequired();

            builder.Property(f => f.Application)
                .HasColumnName("application")
                .HasMaxLength(255);

            builder.Property(f => f.ProcessingError)
                .HasColumnName("processing_error");

            builder.Property(f => f.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(f => f.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(f => f.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(f => new { f.OrganizationId, f.Checksum })
                .IsUnique()
                .HasDatabaseName("ix_files_organization_checksum");

            builder.HasIndex(f => new { f.OrganizationId, f.CreatedAt })
                .HasDatabaseName("ix_files_organization_created");
        }
    }
}
