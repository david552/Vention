using Vention.Domain.Common;
using Vention.Domain.Files;
using Vention.Domain.Organizations;

namespace Vention.Domain.DocumentChunks
{
    public sealed class DocumentChunk : AggregateRoot<DocumentChunkId>
    {
        public StoredFileId FileId { get; private set; }
        public OrganizationId OrganizationId { get; private set; }
        public string Text { get; private set; } = null!;
        public float[] Vector { get; private set; } = null!;
        public int ChunkIndex { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private DocumentChunk() { }

        private DocumentChunk(
            DocumentChunkId id,
            StoredFileId fileId,
            OrganizationId organizationId,
            string text,
            float[] vector,
            int chunkIndex) : base(id)
        {
            FileId = fileId;
            OrganizationId = organizationId;
            Text = text;
            Vector = vector;
            ChunkIndex = chunkIndex;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static DocumentChunk Create(
            StoredFileId fileId,
            OrganizationId organizationId,
            string text,
            float[] vector,
            int chunkIndex)
        {
            if (fileId.Value == Guid.Empty)
                throw new ArgumentException("FileId cannot be empty.", nameof(fileId));

            if (organizationId.Value == Guid.Empty)
                throw new ArgumentException("OrganizationId cannot be empty.", nameof(organizationId));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Chunk text cannot be empty.", nameof(text));

            if (vector is null || vector.Length == 0)
                throw new ArgumentException("Vector embedding cannot be empty.", nameof(vector));

            if (chunkIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex), "Chunk index cannot be negative.");

            return new DocumentChunk(
                new DocumentChunkId(Guid.NewGuid()),
                fileId,
                organizationId,
                text.Trim(),
                vector,
                chunkIndex);
        }
    }

    public record DocumentChunkId(Guid Value);
}