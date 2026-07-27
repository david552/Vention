namespace Vention.Application.Abstractions
{

    public interface IFileStorageService
    {

        Task<FileStorageResult> SaveAsync(
            Stream content,
            Guid organizationId,
            string extension,
            long maxSizeBytes,
            CancellationToken ct = default);

        Task DeleteAsync(string storageKey, CancellationToken ct = default);
    }

    public sealed record FileStorageResult(string StorageKey, long Size, string Checksum);
}