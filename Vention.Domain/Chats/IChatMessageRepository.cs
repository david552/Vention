namespace Vention.Domain.Chats
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage?> GetByIdAsync(ChatMessageId id, CancellationToken ct);
        Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(ChatSessionId sessionId, CancellationToken ct);
        void Add(ChatMessage chatMessage);
        void Remove(ChatMessage chatMessage);
    }
}
