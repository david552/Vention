using System.Runtime.CompilerServices;
using Vention.Application.Abstractions.DocumentChunks;

namespace Vention.Infrastructure.Services.DocumentExtraction.Strategies;
public sealed class TxtExtractionStrategy : IExtractionStrategy
{
    private const int ReadBufferChars = 4096;

    public bool CanHandle(string contentType)
    {
        return string.Equals(contentType?.Trim(), "text/plain", StringComparison.OrdinalIgnoreCase);
    }

    public async IAsyncEnumerable<string> ExtractAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var reader = new StreamReader(
            stream,
            encoding: System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: true);

        var buffer = new char[ReadBufferChars];

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var read = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);

            if (read == 0)
                yield break;

            yield return new string(buffer, 0, read);
        }
    }
}
