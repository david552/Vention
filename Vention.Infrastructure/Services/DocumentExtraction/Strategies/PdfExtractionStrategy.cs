using System.Runtime.CompilerServices;
using System.Text;

using UglyToad.PdfPig;

using Vention.Application.Abstractions.DocumentChunks;
using Vention.Application.Exceptions;

namespace Vention.Infrastructure.Services.DocumentExtraction.Strategies;

public sealed class PdfExtractionStrategy : IExtractionStrategy
{
    public bool CanHandle(string contentType)
        => string.Equals(contentType?.Trim(), "application/pdf", StringComparison.OrdinalIgnoreCase);

    public async IAsyncEnumerable<string> ExtractAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Stream readable = stream;
        FileStream? tempFileStream = null;

        // NOTE: Currently, local storage returns seekable streams, so this block won't execute.
        // It is kept as a future-proof safeguard for remote storage integrations (e.g., AWS S3), 
        // where incoming network streams are non-seekable and must be buffered locally first.
        if (!stream.CanSeek)
        {
            var tempFilePath = Path.GetTempFileName();

            tempFileStream = new FileStream(
                tempFilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 81920, 
                FileOptions.Asynchronous | FileOptions.DeleteOnClose);

            await stream.CopyToAsync(tempFileStream, ct);

            tempFileStream.Position = 0;
            readable = tempFileStream;
        }
        else if (stream.CanSeek && stream.Position != 0)
        {
            stream.Position = 0;
        }

        var yieldedAny = false;

        try
        {
            using var document = PdfDocument.Open(readable);

            foreach (var page in document.GetPages())
            {
                ct.ThrowIfCancellationRequested();

                var text = page.Text;
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                yieldedAny = true;
                yield return text;

                await Task.Yield();

            }
        }
        finally
        {

            tempFileStream?.Dispose();
        }

        if (!yieldedAny)
            throw new PermanentIngestionException("No extractable text was found in the PDF.");
    }
}