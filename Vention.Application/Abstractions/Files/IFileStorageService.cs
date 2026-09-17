namespace Vention.Application.Abstractions.Files
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
        Task<Stream> GetStreamAsync(string storageKey, CancellationToken ct = default);
    }

    public sealed record FileStorageResult(string StorageKey, long Size, string Checksum);
}