namespace Vention.Application.Abstractions.DocumentChunks;
public interface IExtractionStrategy
{
    bool CanHandle(string contentType);
    IAsyncEnumerable<string> ExtractAsync(Stream stream, CancellationToken ct);
}
