using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vention.Domain.Organizations;

namespace Vention.Infrastructure.Persistence.Repositories
{
    public sealed class OrganizationRepository : IOrganizationRepository
    {
        private readonly VentionDbContext _context;
        public OrganizationRepository(VentionDbContext context) => _context = context;

        public Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken ct)
            => _context.Organizations.FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken ct)
            => await _context.Organizations.AsNoTracking().ToListAsync(ct);

        public Task<bool> ExistsByIdAsync(OrganizationId id, CancellationToken ct)
            => _context.Organizations.AnyAsync(o => o.Id == id, ct);

        public void Add(Organization organization) => _context.Organizations.Add(organization);
    }
}
