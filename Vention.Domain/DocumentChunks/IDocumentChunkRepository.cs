using Vention.Domain.Files;
using Vention.Domain.Organizations;

namespace Vention.Domain.DocumentChunks;
public interface IDocumentChunkRepository
{
    Task<IReadOnlyList<DocumentChunk>> GetByFileIdAsync(StoredFileId fileId, CancellationToken ct);

    Task<IReadOnlyList<DocumentChunk>> SearchSimilarAsync(
        OrganizationId organizationId,
        float[] queryEmbedding,
        int topK,
        StoredFileId? fileId,
        double maxCosineDistance,
        CancellationToken ct);
    void AddRange(IReadOnlyList<DocumentChunk> chunks);
    void RemoveRange(IReadOnlyList<DocumentChunk> chunks);
    Task DeleteByFileIdAsync(StoredFileId fileId, CancellationToken ct = default);
}
