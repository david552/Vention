using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vention.Domain.Chats;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Configurations
{
    public sealed class ChatSessionMemberConfiguration : IEntityTypeConfiguration<ChatSessionMember>
    {
        public void Configure(EntityTypeBuilder<ChatSessionMember> builder)
        {
            builder.ToTable("chat_session_members");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new ChatSessionMemberId(value))
                .ValueGeneratedNever();

            builder.Property(m => m.ChatSessionId)
                .HasColumnName("chat_session_id")
                .HasConversion(id => id.Value, value => new ChatSessionId(value))
                .IsRequired();

            builder.Property(m => m.UserId)
                .HasColumnName("user_id")
                .HasConversion(id => id.Value, value => new UserId(value))
                .IsRequired();

            builder.Property(m => m.JoinedAt)
                .HasColumnName("joined_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasOne<ChatSession>()
                .WithMany()
                .HasForeignKey(m => m.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => new { m.ChatSessionId, m.UserId })
                .IsUnique();

            builder.HasIndex(m => m.UserId);
        }
    }
}
