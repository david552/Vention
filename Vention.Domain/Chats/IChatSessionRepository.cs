using Vention.Domain.Organizations;

namespace Vention.Domain.Chats
{
    public interface IChatSessionRepository
    {
        Task<ChatSession?> GetByIdAsync(ChatSessionId id, CancellationToken ct);
        Task<IReadOnlyList<ChatSession>> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken ct);
        Task<bool> ExistsByIdAsync(ChatSessionId id, CancellationToken ct);
        Task DeleteByOrganizationIdAsync(OrganizationId organizationId, CancellationToken ct);
        void Add(ChatSession chatSession);
        void Remove(ChatSession chatSession);
    }
}
