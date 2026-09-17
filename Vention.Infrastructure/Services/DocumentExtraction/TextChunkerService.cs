using System.Runtime.CompilerServices;
using System.Text;

using Vention.Application.Abstractions.DocumentChunks;

namespace Vention.Infrastructure.Services.DocumentExtraction;

public sealed class TextChunkerService : ITextChunker
{
    public async IAsyncEnumerable<string> ChunkTextAsync(
        IAsyncEnumerable<string> textBlocks,
        int chunkSize = 1000,
        int overlap = 200,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize));

        if (overlap < 0 || overlap >= chunkSize)
            throw new ArgumentOutOfRangeException(nameof(overlap), "Overlap must be >= 0 and < chunkSize.");

        var buffer = new StringBuilder();

        await foreach (var block in textBlocks.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(block))
                continue;

            buffer.Append(block.Replace("\r\n", "\n"));

            while (buffer.Length >= chunkSize)
            {
                ct.ThrowIfCancellationRequested();

                var splitAt = FindSplitIndex(buffer, chunkSize);
                var chunk = buffer.ToString(0, splitAt).Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                    yield return chunk;

                var removeCount = Math.Max(splitAt - overlap, 1);
                buffer.Remove(0, removeCount);
            }
        }

        if (buffer.Length > 0)
        {
            var tail = buffer.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(tail))
                yield return tail;
        }
    }

    private static int FindSplitIndex(StringBuilder buffer, int chunkSize)
    {
        var window = Math.Min(buffer.Length, chunkSize);
        var minSplit = Math.Max(window / 2, 1);

        var paragraph = LastIndexOf(buffer, "\n\n", minSplit, window);
        if (paragraph >= minSplit)
            return paragraph + 2;

        for (var i = window - 1; i >= minSplit; i--)
        {
            var c = buffer[i];
            if (c is '.' or '!' or '?' or '。')
            {
                var next = i + 1;
                if (next >= window || char.IsWhiteSpace(buffer[next]) || buffer[next] == '\n')
                    return Math.Min(next + (next < window && char.IsWhiteSpace(buffer[next]) ? 1 : 0), window);
            }
        }

        for (var i = window - 1; i >= minSplit; i--)
        {
            if (char.IsWhiteSpace(buffer[i]))
                return i + 1;
        }

        return window;
    }

    private static int LastIndexOf(StringBuilder buffer, string value, int start, int end)
    {
        for (var i = end - value.Length; i >= start; i--)
        {
            var match = true;
            for (var j = 0; j < value.Length; j++)
            {
                if (buffer[i + j] != value[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return i;
        }

        return -1;
    }
}