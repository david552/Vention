using Vention.Domain.Users;

namespace Vention.Domain.Organizations
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken ct);
        Task<IReadOnlyList<Organization>> GetByIdsAsync(IReadOnlyCollection<OrganizationId> ids, UserId actingUserId, CancellationToken ct);
        Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken ct);
        void Add(Organization organization);
        Task<bool> ExistsByIdAsync(OrganizationId id, CancellationToken ct);

    }
}
