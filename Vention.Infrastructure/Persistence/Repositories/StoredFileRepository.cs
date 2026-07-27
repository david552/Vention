using Microsoft.EntityFrameworkCore;
using Vention.Domain.Files;
using Vention.Domain.Organizations;

namespace Vention.Infrastructure.Persistence.Repositories
{
    public sealed class StoredFileRepository : IStoredFileRepository
    {
        private readonly VentionDbContext _context;
        public StoredFileRepository(VentionDbContext context) => _context = context;

        public Task<StoredFile?> GetByIdAsync(StoredFileId id, CancellationToken ct)
            => _context.StoredFiles.FirstOrDefaultAsync(f => f.Id == id, ct);

        public Task<StoredFile?> GetByOrganizationAndChecksumAsync(
            OrganizationId organizationId,
            string checksum,
            CancellationToken ct)
            => _context.StoredFiles.FirstOrDefaultAsync(
                f => f.OrganizationId == organizationId && f.Checksum == checksum, ct);

        public async Task<IReadOnlyList<StoredFile>> GetByOrganizationAsync(
            OrganizationId organizationId,
            int take,
            CancellationToken ct)
            => await _context.StoredFiles
                .AsNoTracking()
                .Where(f => f.OrganizationId == organizationId)
                .OrderByDescending(f => f.CreatedAt)
                .Take(take)
                .ToListAsync(ct);

        public void Add(StoredFile storedFile) => _context.StoredFiles.Add(storedFile);
        public void Remove(StoredFile storedFile) => _context.StoredFiles.Remove(storedFile);
    }
}