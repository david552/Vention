namespace Vention.Application.Abstractions.Llm
{
    public interface ILlmService
    {
        Task<string> ChatAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken ct = default);
    }
}