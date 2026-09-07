using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Domain.Chats
{
    public interface IChatSessionMemberRepository
    {
        Task<IReadOnlyList<ChatSessionMember>> GetBySessionIdAsync(ChatSessionId sessionId, CancellationToken ct);

        Task<IReadOnlyList<(ChatSession Session, long Sequence)>> GetSessionsForUserPageAsync(
            UserId userId,
            OrganizationId organizationId,
            DateTimeOffset? cursorUpdatedAt,
            long? cursorSequence,
            int take,
            CancellationToken ct);

        Task<IReadOnlyList<ChatSession>> GetSessionsForUserAsync(
            UserId userId,
            OrganizationId organizationId,
            CancellationToken ct);

        Task<ChatSessionMember?> GetMembershipAsync(
            ChatSessionId sessionId,
            UserId userId,
            CancellationToken ct);

        Task<ChatSession?> FindDirectSessionAsync(
            UserId userA,
            UserId userB,
            OrganizationId organizationId,
            CancellationToken ct);

        Task<bool> IsMemberAsync(ChatSessionId sessionId, UserId userId, CancellationToken ct);

        void Add(ChatSessionMember member);
        void Remove(ChatSessionMember member);
    }
}
