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
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private ChatSession() { } 

        private ChatSession(ChatSessionId id, string title, OrganizationId organizationId, UserId createdByUserId) : base(id)
        {
            Title = title;
            OrganizationId = organizationId;
            CreatedByUserId = createdByUserId;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static ChatSession Create(string title, OrganizationId organizationId, UserId createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            if (organizationId.Value == Guid.Empty)
                throw new ArgumentException("OrganizationId cannot be empty.", nameof(organizationId));

            if (createdByUserId.Value== Guid.Empty)
                throw new ArgumentException("CreatedByUserId cannot be empty.", nameof(createdByUserId));

            return new ChatSession(new ChatSessionId(Guid.NewGuid()), title.Trim(), organizationId, createdByUserId);
        }

        public void Rename(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            Title = title.Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public record ChatSessionId(Guid Value);
}
