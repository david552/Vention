using Vention.Domain.Chats;
using Vention.Domain.Users;

namespace Vention.Domain.Messages
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage?> GetByIdAsync(ChatMessageId id, CancellationToken ct);

        Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(
            ChatSessionId sessionId,
            CancellationToken ct);

        Task<IReadOnlyList<(ChatMessage Message, long Sequence)>> GetPageBySessionIdAsync(
            ChatSessionId sessionId,
            DateTimeOffset? cursorCreatedAt,
            long? cursorSequence,
            int take,
            CancellationToken ct);

        Task<ChatMessage?> GetLatestBySessionIdAsync(ChatSessionId sessionId, CancellationToken ct);

        Task<int> CountUnreadAsync(
            ChatSessionId sessionId,
            UserId readerId,
            DateTimeOffset? lastReadAt,
            CancellationToken ct);

        void Add(ChatMessage chatMessage);
        void Remove(ChatMessage chatMessage);
    }
}
