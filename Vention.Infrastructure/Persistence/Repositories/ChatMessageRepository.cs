using Microsoft.EntityFrameworkCore;
using Vention.Domain.Chats;

namespace Vention.Infrastructure.Persistence.Repositories
{
    public sealed class ChatMessageRepository : IChatMessageRepository
    {
        private readonly VentionDbContext _context;
        public ChatMessageRepository(VentionDbContext context) => _context = context;

        public Task<ChatMessage?> GetByIdAsync(ChatMessageId id, CancellationToken ct)
            => _context.ChatMessages.FirstOrDefaultAsync(m => m.Id == id, ct);

        public async Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(ChatSessionId sessionId, CancellationToken ct)
            => await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(ct);

        public void Add(ChatMessage chatMessage) => _context.ChatMessages.Add(chatMessage);
        public void Remove(ChatMessage chatMessage) => _context.ChatMessages.Remove(chatMessage);
    }
}
