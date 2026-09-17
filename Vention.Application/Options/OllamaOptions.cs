using System.ComponentModel.DataAnnotations;

namespace Vention.Application.Options
{
    public sealed class OllamaOptions
    {
        public const string SectionName = "Ollama";

        public const int DefaultEmbeddingDimensions = 768;

        [Required(AllowEmptyStrings = false)]
        public string BaseUrl { get; set; } = "http://localhost:11434";

        [Required(AllowEmptyStrings = false)]
        public string EmbeddingModel { get; set; } = "nomic-embed-text";

        [Required(AllowEmptyStrings = false)]
        public string ChatModel { get; set; } = "llama3.2";

        [Range(1, 300)]
        public int TimeoutSeconds { get; set; } = 120;

        [Range(1, 16)]
        public int MaxConcurrentRequests { get; set; } = 1;

        [Range(1, 4096)]
        public int EmbeddingDimensions { get; set; } = DefaultEmbeddingDimensions;
    }
}