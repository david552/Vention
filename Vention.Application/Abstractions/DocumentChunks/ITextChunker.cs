using System.Collections.Generic;

namespace Vention.Application.Abstractions.DocumentChunks;
public interface ITextChunker
{
    IAsyncEnumerable<string> ChunkTextAsync(IAsyncEnumerable<string> text, int chunkSize = 1000, int overlap = 200, CancellationToken cancellationToken = default);
}
