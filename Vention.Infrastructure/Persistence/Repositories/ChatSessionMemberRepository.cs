using Microsoft.EntityFrameworkCore;
using Vention.Domain.Chats;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Repositories
{
    public sealed class ChatSessionMemberRepository : IChatSessionMemberRepository
    {
        private readonly VentionDbContext _context;
        public ChatSessionMemberRepository(VentionDbContext context) => _context = context;

        public async Task<IReadOnlyList<ChatSessionMember>> GetBySessionIdAsync(ChatSessionId sessionId, CancellationToken ct)
            => await _context.ChatSessionMembers
                .AsNoTracking()
                .Where(m => m.ChatSessionId == sessionId)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<(ChatSession Session, long Sequence)>> GetSessionsForUserPageAsync(
           UserId userId,
           OrganizationId organizationId,
           DateTimeOffset? cursorUpdatedAt,
           long? cursorSequence,
           int take,
           CancellationToken ct)
        {
            var query = _context.ChatSessions
                .AsNoTracking()
                .Where(cs => cs.OrganizationId == organizationId)
                .Where(cs => _context.ChatSessionMembers
                    .Any(m => m.ChatSessionId == cs.Id && m.UserId == userId));

            if (cursorUpdatedAt is not null && cursorSequence is not null)
            {
                query = query.Where(cs =>
                    cs.UpdatedAt < cursorUpdatedAt ||
                    (cs.UpdatedAt == cursorUpdatedAt &&
                     EF.Property<long>(cs, "Sequence") < cursorSequence));
            }

            var rows = await query
                .OrderByDescending(cs => cs.UpdatedAt)
                .ThenByDescending(cs => EF.Property<long>(cs, "Sequence"))
                .Take(take)
                .Select(cs => new { Session = cs, Sequence = EF.Property<long>(cs, "Sequence") })
                .ToListAsync(ct);

            return rows.Select(r => (r.Session, r.Sequence)).ToList();
        }


        public Task<ChatSession?> FindDirectSessionAsync(
           UserId userA,
           UserId userB,
           OrganizationId organizationId,
           CancellationToken ct)
        {
            var key = DirectChatKey.Create(userA, userB).Value;

            return _context.ChatSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    cs => cs.OrganizationId == organizationId
                       && cs.DirectChatKey != null
                       && cs.DirectChatKey.Value == key,
                    ct);
        }

        public Task<bool> IsMemberAsync(ChatSessionId sessionId, UserId userId, CancellationToken ct)
            => _context.ChatSessionMembers
                .AnyAsync(m => m.ChatSessionId == sessionId && m.UserId == userId, ct);

        public void Add(ChatSessionMember member) => _context.ChatSessionMembers.Add(member);
        public void Remove(ChatSessionMember member) => _context.ChatSessionMembers.Remove(member);
    }
}
