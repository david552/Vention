namespace Vention.Application.Abstractions.Llm
{
    public interface IEmbeddingService
    {
        Task<IReadOnlyList<float[]>> EmbedAsync(
            IReadOnlyList<string> inputs,
            CancellationToken ct = default);
    }
}