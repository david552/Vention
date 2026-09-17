namespace Vention.Application.Abstractions.DocumentChunks;
public interface IDocumentExtractor
{
    IAsyncEnumerable<string> ExtractTextAsync(Stream fileStream, string contentType, CancellationToken ct);
}
