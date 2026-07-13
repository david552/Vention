using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Domain.Membership
{
    public interface IMembershipRepository
    {
        Task<Membership?> GetByIdAsync(MembershipId id, CancellationToken ct);
        Task<Membership?> GetByUserAndOrganizationAsync(UserId userId, OrganizationId organizationId, CancellationToken ct);
        Task<IReadOnlyList<Membership>> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken ct);
        Task<IReadOnlyList<Membership>> GetByUserIdAsync(UserId userId, CancellationToken ct);
        Task<bool> ExistsAsync(UserId userId, OrganizationId organizationId, CancellationToken ct);
        void Add(Membership membership);
        void Remove(Membership membership);
    }
}
