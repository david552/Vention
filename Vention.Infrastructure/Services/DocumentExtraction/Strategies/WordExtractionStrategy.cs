using System.Runtime.CompilerServices;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using Vention.Application.Abstractions.DocumentChunks;
using Vention.Application.Exceptions;

namespace Vention.Infrastructure.Services.DocumentExtraction.Strategies;

public sealed class WordExtractionStrategy : IExtractionStrategy
{
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public bool CanHandle(string contentType)
    {
        var ct = contentType?.Trim();

        return string.Equals(
            ct,
            DocxContentType,
            StringComparison.OrdinalIgnoreCase);
    }

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
            using var wordDoc = WordprocessingDocument.Open(readable, false);
            var body = wordDoc.MainDocumentPart?.Document?.Body
                ?? throw new PermanentIngestionException("The Word document has no body content.");

            foreach (var paragraph in body.Elements<Paragraph>())
            {
                ct.ThrowIfCancellationRequested();

                var line = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text)).Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                yieldedAny = true;

                yield return line;

                await Task.Yield();
            }

            if (!yieldedAny)
            {
                foreach (var text in body.Descendants<Text>())
                {
                    ct.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(text.Text))
                        continue;

                    yieldedAny = true;
                    yield return text.Text;

                    await Task.Yield();
                }
            }
        }
        finally
        {
            tempFileStream?.Dispose();
        }

        if (!yieldedAny)
            throw new PermanentIngestionException("No extractable text was found in the Word document.");
    }
}