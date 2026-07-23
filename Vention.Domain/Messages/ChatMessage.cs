using Vention.Domain.Chats;
using Vention.Domain.Common;
using Vention.Domain.Users;

namespace Vention.Domain.Messages
{
    public sealed class ChatMessage : AggregateRoot<ChatMessageId>
    {
        public ChatSessionId ChatSessionId { get; private set; }
        public UserId SenderId { get; private set; }
        public string Content { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }

        private ChatMessage() { } 

        private ChatMessage(ChatMessageId id, ChatSessionId chatSessionId, UserId senderId, string content) : base(id)
        {
            ChatSessionId = chatSessionId;
            SenderId = senderId;
            Content = content;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static ChatMessage Create(ChatSessionId chatSessionId, UserId senderId, string content)
        {
            if (chatSessionId.Value == Guid.Empty)
                throw new ArgumentException("ChatSessionId cannot be empty.", nameof(chatSessionId));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be empty.", nameof(content));

            return new ChatMessage(new ChatMessageId(Guid.NewGuid()), chatSessionId, senderId, content);
        }

    }
    public record ChatMessageId(Guid Value);
}
