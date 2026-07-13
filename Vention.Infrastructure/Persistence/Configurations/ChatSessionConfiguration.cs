using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vention.Domain.Chats;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Configurations
{
    public sealed class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.ToTable("chat_sessions");

            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new ChatSessionId(value))
                .ValueGeneratedNever();

            builder.Property(cs => cs.Title)
                .HasColumnName("title")
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(cs => cs.OrganizationId)
                .HasColumnName("organization_id")
                .HasConversion(id => id.Value, value => new OrganizationId(value))
                .IsRequired();

            builder.Property(cs => cs.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .HasConversion(id => id.Value, value => new UserId(value))
                .IsRequired();

            builder.Property(cs => cs.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(cs => cs.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(cs => cs.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(cs => cs.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
