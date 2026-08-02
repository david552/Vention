using MassTransit;
using Microsoft.Extensions.Logging;
using Vention.Application.Files.IntegrationEvents;
using Vention.Domain.Files;

namespace Vention.Processor.Consumers
{
    public sealed class PrepareFileIngestionConsumer : IConsumer<FileIngestionRequested>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly ILogger<PrepareFileIngestionConsumer> _logger;

        public PrepareFileIngestionConsumer(
            IStoredFileRepository storedFileRepository,
            ILogger<PrepareFileIngestionConsumer> logger)
        {
            _storedFileRepository = storedFileRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileIngestionRequested> context)
        {

            var message = context.Message;
            var ct = context.CancellationToken;

            _logger.LogInformation(
                "Ingestion prepare started. FileId={FileId}",
                message.FileId);

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

            _logger.LogInformation(
                "Ingestion prepare completed. Published FileIngestionPrepared. FileId={FileId}",
                message.FileId);
        }
    }
}