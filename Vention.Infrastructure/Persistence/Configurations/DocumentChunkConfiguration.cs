using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using Vention.Application.Options;
using Vention.Domain.DocumentChunks;
using Vention.Domain.Files;
using Vention.Domain.Organizations;

namespace Vention.Infrastructure.Persistence.Configurations
{
    public sealed class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
    {
        public const int EmbeddingDimensions = 768;

        public void Configure(EntityTypeBuilder<DocumentChunk> builder)
        {
            builder.ToTable("document_chunks");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new DocumentChunkId(value))
                .ValueGeneratedNever();

            builder.Property(c => c.FileId)
                .HasColumnName("file_id")
                .HasConversion(id => id.Value, value => new StoredFileId(value))
                .IsRequired();

            builder.Property(c => c.OrganizationId)
                .HasColumnName("organization_id")
                .HasConversion(id => id.Value, value => new OrganizationId(value))
                .IsRequired();

            builder.Property(c => c.Text)
                .HasColumnName("text")
                .IsRequired();

            builder.Property(c => c.Vector)
                .HasColumnName("embedding")
                .HasColumnType($"vector({OllamaOptions.DefaultEmbeddingDimensions})")
                .HasConversion(
                    v => new Vector(v),
                    v => v.ToArray())
                .IsRequired();

            builder.Property(c => c.ChunkIndex)
                .HasColumnName("chunk_index")
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasOne<StoredFile>()
                .WithMany()
                .HasForeignKey(c => c.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(c => c.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => new { c.FileId, c.ChunkIndex })
                .IsUnique()
                .HasDatabaseName("ix_document_chunks_file_chunk_index");

            builder.HasIndex(c => c.OrganizationId)
                .HasDatabaseName("ix_document_chunks_organization_id");

            builder.HasIndex(c => c.Vector)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops")
                .HasDatabaseName("ix_document_chunks_embedding");
        }
    }
}