using Microsoft.EntityFrameworkCore;
using Vention.Domain.Chats;
using Vention.Domain.Messages;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Repositories
{

    public sealed class ChatMessageRepository : IChatMessageRepository
    {
        private readonly VentionDbContext _context;

        public ChatMessageRepository(VentionDbContext context) => _context = context;

        public Task<ChatMessage?> GetByIdAsync(ChatMessageId id, CancellationToken ct)
            => _context.ChatMessages.FirstOrDefaultAsync(m => m.Id == id, ct);

        public async Task<IReadOnlyList<ChatMessage>> GetBySessionIdAsync(
            ChatSessionId sessionId,
            CancellationToken ct)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => EF.Property<long>(m, "Sequence"))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<(ChatMessage Message, long Sequence)>> GetPageBySessionIdAsync(
            ChatSessionId sessionId,
            DateTimeOffset? cursorCreatedAt,
            long? cursorSequence,
            int take,
            CancellationToken ct)
        {
            var q = _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatSessionId == sessionId);

            if (cursorCreatedAt is not null && cursorSequence is not null)
            {
                q = q.Where(m =>
                    m.CreatedAt < cursorCreatedAt ||
                    (m.CreatedAt == cursorCreatedAt &&
                     EF.Property<long>(m, "Sequence") < cursorSequence));
            }

            var rows = await q
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => EF.Property<long>(m, "Sequence"))
                .Take(take)
                .Select(m => new { Message = m, Sequence = EF.Property<long>(m, "Sequence") })
                .ToListAsync(ct);

            rows.Reverse();

            return rows.Select(r => (r.Message, r.Sequence)).ToList();
        }

        public async Task<int> CountUnreadAsync(
            ChatSessionId sessionId,
            UserId readerId,
            DateTimeOffset? lastReadAt,
            CancellationToken ct)
        {
            var query = _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatSessionId == sessionId && m.SenderId != readerId);

            if (lastReadAt is not null)
                query = query.Where(m => m.CreatedAt > lastReadAt);

            return await query.CountAsync(ct);
        }

        public Task<ChatMessage?> GetLatestBySessionIdAsync(ChatSessionId sessionId, CancellationToken ct)
            => _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatSessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => EF.Property<long>(m, "Sequence"))
                .FirstOrDefaultAsync(ct);

        public void Add(ChatMessage chatMessage) => _context.ChatMessages.Add(chatMessage);

        public void Remove(ChatMessage chatMessage) => _context.ChatMessages.Remove(chatMessage);
    }
}