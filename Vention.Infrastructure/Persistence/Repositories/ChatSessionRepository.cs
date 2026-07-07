using Microsoft.EntityFrameworkCore;
using Vention.Domain.Chats;
using Vention.Domain.Organizations;

namespace Vention.Infrastructure.Persistence.Repositories
{
    public sealed class ChatSessionRepository : IChatSessionRepository
    {
        private readonly VentionDbContext _context;
        public ChatSessionRepository(VentionDbContext context) => _context = context;

        public Task<ChatSession?> GetByIdAsync(ChatSessionId id, CancellationToken ct)
            => _context.ChatSessions.FirstOrDefaultAsync(cs => cs.Id == id, ct);

        public async Task<IReadOnlyList<ChatSession>> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken ct)
            => await _context.ChatSessions.AsNoTracking().Where(cs => cs.OrganizationId == organizationId).ToListAsync(ct);

        public Task<bool> ExistsByIdAsync(ChatSessionId id, CancellationToken ct)
            => _context.ChatSessions.AnyAsync(cs => cs.Id == id, ct);

        public void Add(ChatSession chatSession) => _context.ChatSessions.Add(chatSession);
        public void Remove(ChatSession chatSession) => _context.ChatSessions.Remove(chatSession);
    }
}
