using System.Diagnostics;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Vention.Application.Abstractions;
using Vention.Application.Abstractions.DocumentChunks;
using Vention.Application.Abstractions.Files;
using Vention.Application.Abstractions.Llm;
using Vention.Application.Exceptions;
using Vention.Application.Files.IntegrationEvents;
using Vention.Application.Options;
using Vention.Application.Rag;
using Vention.Domain.DocumentChunks;
using Vention.Domain.Files;

namespace Vention.Processor.Consumers
{
    public sealed class PrepareFileIngestionConsumer : IConsumer<FileIngestionRequested>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IEmbeddingService _embeddingService;
        private readonly IDocumentExtractor _documentExtractor;
        private readonly ITextChunker _textChunker;
        private readonly IDocumentChunkRepository _documentChunkRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RagOptions _ragOptions;
        private readonly ILogger<PrepareFileIngestionConsumer> _logger;

        public PrepareFileIngestionConsumer(
            IStoredFileRepository storedFileRepository,
            ILogger<PrepareFileIngestionConsumer> logger,
            IEmbeddingService embeddingService,
            IDocumentExtractor documentExtractor,
            ITextChunker textChunker,
            IDocumentChunkRepository documentChunkRepository,
            IFileStorageService fileStorageService,
            IOptions<RagOptions> ragOptions,
            IUnitOfWork unitOfWork)
        {
            _embeddingService = embeddingService;
            _documentExtractor = documentExtractor;
            _textChunker = textChunker;
            _documentChunkRepository = documentChunkRepository;
            _storedFileRepository = storedFileRepository;
            _logger = logger;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _ragOptions = ragOptions.Value;
        }

        public async Task Consume(ConsumeContext<FileIngestionRequested> context)
        {
            var sw = Stopwatch.StartNew();

            var message = context.Message;
            var ct = context.CancellationToken;

            _logger.LogInformation("Ingestion prepare started. FileId={FileId}", message.FileId);

            var file = await _storedFileRepository.GetByIdAsync(new StoredFileId(message.FileId), ct);
            if (file is null)
            {
                _logger.LogWarning("Ingestion prepare skipped: file not found. FileId={FileId}", message.FileId);
                return;
            }

            if (file.Status == FileStatus.Processed)
            {
                _logger.LogInformation("Ingestion prepare skipped: already Processed. FileId={FileId}", message.FileId);
                return;
            }

            try
            {
                await _documentChunkRepository.DeleteByFileIdAsync(file.Id, ct);

                await using var fileStream = await _fileStorageService.GetStreamAsync(file.StorageKey, ct);

                var textBlocks = _documentExtractor.ExtractTextAsync(fileStream, file.ContentType, ct);

                var chunkStream = _textChunker.ChunkTextAsync(
                    textBlocks,
                    _ragOptions.ChunkSize,
                    _ragOptions.ChunkOverlap,
                    ct);

                var batchSize = _ragOptions.EmbeddingBatchSize;
                var batch = new List<string>(batchSize);
                var chunkIndex = 0;
                var totalChunks = 0;

                await foreach (var chunk in chunkStream.WithCancellation(ct))
                {
                    batch.Add(chunk);

                    if (batch.Count < batchSize)
                        continue;

                    chunkIndex = await PersistBatchAsync(file, batch, chunkIndex, ct);
                    totalChunks += batch.Count;
                    batch.Clear();
                }
                if (batch.Count > 0)
                {
                    chunkIndex = await PersistBatchAsync(file, batch, chunkIndex, ct);
                    totalChunks += batch.Count;
                    batch.Clear();
                }
                if (totalChunks == 0)
                    throw new PermanentIngestionException("Text extraction produced no chunks.");

                await context.Publish(
                    new FileIngestionPrepared(
                        file.Id.Value,
                        file.OrganizationId.Value,
                        file.OwnerId.Value,
                        file.Filename,
                        file.Checksum,
                        file.StorageKey,
                        file.ContentType,
                        file.Size,
                        DateTimeOffset.UtcNow),
                    ct);

                await _unitOfWork.SaveChangesAsync(ct);

                sw.Stop();
                RagMetrics.RecordIngest(sw.Elapsed.TotalMilliseconds, totalChunks, "success");

                _logger.LogInformation(
                    "Ingestion prepare completed. FileId={FileId}, Chunks={ChunkCount}",
                    message.FileId,
                    totalChunks);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (IsTransient(ex) && context.GetRetryAttempt() < 4)
            {
                sw.Stop();
                RagMetrics.RecordIngest(sw.Elapsed.TotalMilliseconds, 0, "transient");

                _logger.LogWarning(
                    ex,
                    "Ingestion prepare transient failure; will retry. FileId={FileId}, Attempt={Attempt}",
                    message.FileId,
                    context.GetRetryAttempt());

                _unitOfWork.ClearChangeTracker();
                throw;
            }
            catch (Exception ex)
            {
                sw.Stop();
                RagMetrics.RecordIngest(sw.Elapsed.TotalMilliseconds, 0, "permanent");

                _logger.LogError(ex, "Ingestion prepare permanent failure. FileId={FileId}", message.FileId);

                _unitOfWork.ClearChangeTracker();

                try
                {
                    await _documentChunkRepository.DeleteByFileIdAsync(new StoredFileId(message.FileId), ct);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(
                        cleanupEx,
                        "Failed to clean chunks after permanent ingestion error. FileId={FileId}",
                        message.FileId);
                }

                var failedFile = await _storedFileRepository.GetByIdAsync(new StoredFileId(message.FileId), ct);

                if (failedFile is not null && failedFile.Status != FileStatus.Error)
                {
                    failedFile.MarkFailed(ex.Message);

                    await context.Publish(
                        new FileStatusChanged(
                            failedFile.Id.Value,
                            failedFile.OrganizationId.Value,
                            failedFile.OwnerId.Value,
                            FileStatus.Error,
                            failedFile.Filename,
                            DateTimeOffset.UtcNow),
                        ct);

                    await _unitOfWork.SaveChangesAsync(ct);
                }
            }
        }

        private async Task<int> PersistBatchAsync(
            StoredFile file,
            List<string> batch,
            int startingChunkIndex,
            CancellationToken ct)
        {
            var embeddings = await _embeddingService.EmbedAsync(batch, ct);

            var entities = new List<DocumentChunk>(batch.Count);
            for (var i = 0; i < batch.Count; i++)
            {
                entities.Add(DocumentChunk.Create(
                    file.Id,
                    file.OrganizationId,
                    batch[i],
                    embeddings[i],
                    startingChunkIndex + i));
            }

            _documentChunkRepository.AddRange(entities);

            await _unitOfWork.SaveChangesAsync(ct);
            _unitOfWork.ClearChangeTracker();

            return startingChunkIndex + batch.Count;
        }

        private static bool IsTransient(Exception ex)
        {
            for (var e = ex; e is not null; e = e.InnerException)
            {
                switch (e)
                {
                    case PermanentIngestionException:
                        return false;
                    case NotSupportedException:
                        return false;
                    case ArgumentException:
                        return false;
                    case HttpRequestException:
                        return true;
                    case TaskCanceledException:
                        return true;
                    case TimeoutException:
                        return true;
                    case IOException:
                        return true;
                }
            }
             
            return false;
        }
    }
}