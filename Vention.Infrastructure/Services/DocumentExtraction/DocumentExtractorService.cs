using Vention.Application.Abstractions.DocumentChunks;
using System.Runtime.CompilerServices;


namespace Vention.Infrastructure.Services.DocumentExtraction;
public sealed class DocumentExtractorService : IDocumentExtractor
{
    private readonly IEnumerable<IExtractionStrategy> _strategies;

    public DocumentExtractorService(IEnumerable<IExtractionStrategy> strategies)
        => _strategies = strategies; 

    public async IAsyncEnumerable<string> ExtractTextAsync(
        Stream fileStream,
        string contentType,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(contentType));
        
        if(strategy == null)
        {
            throw new NotSupportedException($"Content type '{contentType}' is not supported for text extraction.");
        }

        await foreach(var block in strategy.ExtractAsync(fileStream, ct).WithCancellation(ct))
        {
            if (!string.IsNullOrWhiteSpace(block))
                yield return block;
        }

    }
}
