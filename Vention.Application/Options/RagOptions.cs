using System.ComponentModel.DataAnnotations;

namespace Vention.Application.Options;
public sealed class RagOptions
{
    public const string SectionName = "Rag";

    [Range(1, 50)]
    public int TopK { get; set; } = 5;

    [Range(100, 8000)]
    public int ChunkSize { get; set; } = 1000;

    [Range(0, 4000)]
    public int ChunkOverlap { get; set; } = 200;

    [Range(1, 100)]
    public int EmbeddingBatchSize { get; set; } = 20;

    [Range(0.0, 2.0)]
    public double MaxCosineDistance { get; set; } = 0.45;

    [Range(500, 100_000)]
    public int MaxContextChars { get; set; } = 6000;

    [Range(0, 4000)]
    public int SourceSnippetChars { get; set; } = 300;
}
