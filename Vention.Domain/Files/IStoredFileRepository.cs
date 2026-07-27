using Vention.Domain.Organizations;

namespace Vention.Domain.Files
{
    public interface IStoredFileRepository
    {
        Task<StoredFile?> GetByIdAsync(StoredFileId id, CancellationToken ct);

        Task<StoredFile?> GetByOrganizationAndChecksumAsync(
            OrganizationId organizationId,
            string checksum,
            CancellationToken ct);

        Task<IReadOnlyList<StoredFile>> GetByOrganizationAsync(
            OrganizationId organizationId,
            int take,
            CancellationToken ct);

        void Add(StoredFile storedFile);
        void Remove(StoredFile storedFile);
    }
}
