using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
