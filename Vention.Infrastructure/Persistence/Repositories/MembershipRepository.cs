using Microsoft.EntityFrameworkCore;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Repositories
{
    public sealed class MembershipRepository : IMembershipRepository
    {
        private readonly VentionDbContext _context;
        public MembershipRepository(VentionDbContext context) => _context = context;

        public Task<Membership?> GetByIdAsync(MembershipId id, CancellationToken ct)
            => _context.Memberships.FirstOrDefaultAsync(m => m.Id == id, ct);

        public async Task<IReadOnlyList<Membership>> GetByUserIdsAsync(
            IReadOnlyCollection<UserId> userIds,
            UserId actingUserId,
            CancellationToken ct)
        {
            if (userIds.Count == 0)
                return Array.Empty<Membership>();

            return await _context.Memberships
                .AsNoTracking()
                .Where(m =>
                    userIds.Contains(m.UserId) &&
                    _context.Memberships.Any(mine =>
                        mine.UserId == actingUserId &&
                        mine.OrganizationId == m.OrganizationId))
                .ToListAsync(ct);
        }
        public Task<Membership?> GetByUserAndOrganizationAsync(UserId userId, OrganizationId organizationId, CancellationToken ct)
            => _context.Memberships.FirstOrDefaultAsync(m => m.UserId == userId && m.OrganizationId == organizationId, ct);

        public async Task<IReadOnlyList<Membership>> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken ct)
            => await _context.Memberships.AsNoTracking().Where(m => m.OrganizationId == organizationId).ToListAsync(ct);

        public async Task<IReadOnlyList<Membership>> GetByUserIdAsync(UserId userId, CancellationToken ct)
            => await _context.Memberships.AsNoTracking().Where(m => m.UserId == userId).ToListAsync(ct);

        public Task<bool> ExistsAsync(UserId userId, OrganizationId organizationId, CancellationToken ct)
            => _context.Memberships.AnyAsync(m => m.UserId == userId && m.OrganizationId == organizationId, ct);

        public Task DeleteByOrganizationIdAsync(OrganizationId organizationId, CancellationToken ct)
            => _context.Memberships.Where(m => m.OrganizationId == organizationId).ExecuteDeleteAsync(ct);

        public void Add(Membership membership) => _context.Memberships.Add(membership);
        public void Remove(Membership membership) => _context.Memberships.Remove(membership);
    }
}
