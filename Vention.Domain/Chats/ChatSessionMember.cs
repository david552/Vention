using Vention.Domain.Common;
using Vention.Domain.Users;

namespace Vention.Domain.Chats
{
    public sealed class ChatSessionMember : Entity<ChatSessionMemberId>
    {
        public ChatSessionId ChatSessionId { get; private set; }
        public UserId UserId { get; private set; }
        public DateTimeOffset JoinedAt { get; private set; }
        public DateTimeOffset? LastReadAt { get; private set; }


        private ChatSessionMember() { }

        private ChatSessionMember(
            ChatSessionMemberId id,
            ChatSessionId chatSessionId,
            UserId userId) : base(id)
        {
            ChatSessionId = chatSessionId;
            UserId = userId;
            JoinedAt = DateTimeOffset.UtcNow;
        }

        public static ChatSessionMember Create(ChatSessionId chatSessionId, UserId userId)
        {
            if (chatSessionId.Value == Guid.Empty)
                throw new ArgumentException("ChatSessionId cannot be empty.", nameof(chatSessionId));

            if (userId.Value == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            return new ChatSessionMember(new ChatSessionMemberId(Guid.NewGuid()), chatSessionId, userId);
        }
        public void MarkAsRead(DateTimeOffset readAt)
        {
            if (LastReadAt is null || readAt > LastReadAt)
                LastReadAt = readAt;
        }
    }

    public record ChatSessionMemberId(Guid Value);
}
