namespace Vention.Application.Abstractions.Llm
{
    public interface IOllamaRequestGate
    {
        Task<IDisposable> AcquireAsync(CancellationToken ct = default);
    }
}