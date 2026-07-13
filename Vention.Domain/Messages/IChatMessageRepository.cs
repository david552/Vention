using Vention.Domain.Chats;

namespace Vention.Domain.Messages
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage?> GetByIdAsync(ChatMessageId id, CancellationToken ct);
        Task<IReadOnlyList<(ChatMessage Message, long Sequence)>> GetPageBySessionIdAsync(
            ChatSessionId sessionId,
            DateTimeOffset? cursorCreatedAt,
            long? cursorSequence,
            int take,
            CancellationToken ct);
        void Add(ChatMessage chatMessage);
        void Remove(ChatMessage chatMessage);
    }
}
