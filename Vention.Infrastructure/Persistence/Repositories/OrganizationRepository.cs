using Microsoft.EntityFrameworkCore;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Infrastructure.Persistence.Repositories
{
    public sealed class OrganizationRepository : IOrganizationRepository
    {
        private readonly VentionDbContext _context;
        public OrganizationRepository(VentionDbContext context) => _context = context;

        public Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken ct)
            => _context.Organizations.FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task<IReadOnlyList<Organization>> GetByIdsAsync(
            IReadOnlyCollection<OrganizationId> ids,
            UserId actingUserId,
            CancellationToken ct)
        {
            if (ids.Count == 0)
                return Array.Empty<Organization>();

            return await _context.Organizations
                .AsNoTracking()
                .Where(o => 
                ids.Contains(o.Id) &&
                _context.Memberships.Any(m =>
                    m.OrganizationId == o.Id &&
                    m.UserId == actingUserId))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken ct)
            => await _context.Organizations.AsNoTracking().ToListAsync(ct);

        public Task<bool> ExistsByIdAsync(OrganizationId id, CancellationToken ct)
            => _context.Organizations.AnyAsync(o => o.Id == id, ct);

        public void Add(Organization organization) => _context.Organizations.Add(organization);
    }
}
