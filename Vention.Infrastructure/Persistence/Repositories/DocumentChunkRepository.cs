using Microsoft.EntityFrameworkCore;

using Pgvector;
using Vention.Domain.DocumentChunks;
using Vention.Domain.Files;
using Vention.Domain.Organizations;


namespace Vention.Infrastructure.Persistence.Repositories;
public sealed class DocumentChunkRepository : IDocumentChunkRepository
{

    private readonly VentionDbContext _context;

    public DocumentChunkRepository(VentionDbContext context) => _context = context;

    public async Task<IReadOnlyList<DocumentChunk>> GetByFileIdAsync(StoredFileId fileId, CancellationToken ct)
    {
        return await _context.DocumentChunks
            .Where(c=>c.FileId == fileId)
            .OrderBy(c=>c.ChunkIndex)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DocumentChunk>> SearchSimilarAsync(
        OrganizationId organizationId,
        float[] queryEmbedding,
        int topK,
        StoredFileId? fileId,
        double maxCosineDistance,
        CancellationToken ct)
    {
        if (queryEmbedding is null || queryEmbedding.Length == 0)
            throw new ArgumentException("Query embedding cannot be empty.", nameof(queryEmbedding));

        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(topK));

        var vector = new Vector(queryEmbedding);

        if (fileId is null)
        {
            return await _context.DocumentChunks
                .FromSqlInterpolated($"""
                SELECT id, file_id, organization_id, text, embedding, chunk_index, created_at
                FROM document_chunks
                WHERE organization_id = {organizationId.Value}
                 AND (embedding <=> {vector}) <= {maxCosineDistance}
                ORDER BY embedding <=> {vector}
                LIMIT {topK}
                """)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        var fileIdValue = fileId.Value;

        return await _context.DocumentChunks
            .FromSqlInterpolated($"""
            SELECT id, file_id, organization_id, text, embedding, chunk_index, created_at
            FROM document_chunks
            WHERE organization_id = {organizationId.Value}
              AND file_id = {fileIdValue}
              AND (embedding <=> {vector}) <= {maxCosineDistance}
            ORDER BY embedding <=> {vector}
            LIMIT {topK}
            """)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public void AddRange(IReadOnlyList<DocumentChunk> chunks)
        => _context.DocumentChunks.AddRange(chunks);

    public void RemoveRange(IReadOnlyList<DocumentChunk> chunks)
        => _context.DocumentChunks.RemoveRange(chunks);

    public async Task DeleteByFileIdAsync(StoredFileId fileId, CancellationToken ct = default)
    {
        await _context.DocumentChunks
            .Where(c => c.FileId == fileId)
            .ExecuteDeleteAsync(ct);
    }
}
