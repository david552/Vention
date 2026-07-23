using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vention.Domain.Chats;
using Vention.Domain.Messages;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Configurations
{
    public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("chat_messages");

            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new ChatMessageId(value))
                .ValueGeneratedNever();

            builder.Property(cm => cm.ChatSessionId)
                .HasColumnName("chat_session_id")
                .HasConversion(id => id.Value, value => new ChatSessionId(value))
                .IsRequired();

            builder.Property(cm => cm.SenderId)
                   .HasColumnName("sender_id")
                   .HasConversion(id => id.Value, value => new UserId(value))
                   .IsRequired();

            builder.Property(cm => cm.Content)
                .HasColumnName("content")
                .IsRequired();

            builder.Property<long>("Sequence")
                .HasColumnName("sequence")
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn();

            builder.Property(cm => cm.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ChatSession>()
                .WithMany()
                .HasForeignKey(cm => cm.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(cm => new { cm.ChatSessionId, cm.CreatedAt, cm.Id });

            builder.HasIndex("ChatSessionId", "CreatedAt", "Sequence")
                .HasDatabaseName("ix_chat_messages_session_created_sequence");
        }
    }
}
