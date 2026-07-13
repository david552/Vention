using Vention.Domain.Common;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Domain.Chats
{
    public sealed class ChatSession : AggregateRoot<ChatSessionId>
    {
        public string Title { get; private set; } = null!;
        public OrganizationId OrganizationId { get; private set; }
        public UserId CreatedByUserId { get; private set; }
        public DirectChatKey? DirectChatKey { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private ChatSession() { } 

        private ChatSession(ChatSessionId id, string title, OrganizationId organizationId, UserId createdByUserId, DirectChatKey? directChatKey) : base(id)
        {
            Title = title;
            OrganizationId = organizationId;
            CreatedByUserId = createdByUserId;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            DirectChatKey = directChatKey;
        }

        public static ChatSession CreateDirectChat(OrganizationId organizationId, UserId initiatorId, UserId participantId)
        {
            if (organizationId.Value == Guid.Empty)
                throw new ArgumentException("OrganizationId cannot be empty.", nameof(organizationId));

            var chatKey = Chats.DirectChatKey.Create(initiatorId, participantId);

            return new ChatSession(
                new ChatSessionId(Guid.NewGuid()),
                "Direct Chat",
                organizationId,
                initiatorId,
                chatKey
            );
        }

        public void Rename(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            Title = title.Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Touch()
        {
            UpdatedAt = DateTimeOffset.UtcNow;
        }

    }

    public record ChatSessionId(Guid Value);

    public sealed record DirectChatKey
    {
        public string Value { get; }

        internal DirectChatKey(string value)
        {
            Value = value;
        }

        public static DirectChatKey Create(UserId user1, UserId user2)
        {
            if (user1 == user2)
            {
                throw new InvalidOperationException("Cannot create a chat key for the same user.");
            }

            string key = user1.Value.CompareTo(user2.Value) < 0
                ? $"{user1.Value}_{user2.Value}"
                : $"{user2.Value}_{user1.Value}";

            return new DirectChatKey(key);
        }
    }
}
