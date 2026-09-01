using MassTransit;
using Microsoft.Extensions.Logging;
using Vention.Application.Abstractions;
using Vention.Application.Files.IntegrationEvents;
using Vention.Domain.Files;

namespace Vention.Processor.Consumers
{
    public sealed class CompleteFileIngestionConsumer : IConsumer<FileIngestionPrepared>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CompleteFileIngestionConsumer> _logger;

        public CompleteFileIngestionConsumer(
            IStoredFileRepository storedFileRepository,
            IUnitOfWork unitOfWork,
            ILogger<CompleteFileIngestionConsumer> logger)
        {
            _storedFileRepository = storedFileRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileIngestionPrepared> context)
        {
            var message = context.Message;
            var ct = context.CancellationToken;

            await Task.Delay(5000);
            _logger.LogInformation(
                "Ingestion complete started. FileId={FileId}",
                message.FileId);

            var file = await _storedFileRepository.GetByIdAsync(new StoredFileId(message.FileId), ct);
            if (file is null)
            {
                _logger.LogWarning("Ingestion complete skipped: file not found. FileId={FileId}", message.FileId);
                return;
            }

            if (file.Status == FileStatus.Processed)
            {
                _logger.LogInformation("Ingestion complete skipped: already Processed. FileId={FileId}", message.FileId);
                return;
            }


            file.MarkProcessed();

            await context.Publish(
                new FileStatusChanged(
                    message.FileId,
                    message.OrganizationId,
                    message.OwnerId,
                    FileStatus.Processed,
                    message.Filename,
                    DateTimeOffset.UtcNow),
                ct);

            await _unitOfWork.SaveChangesAsync(ct);



            _logger.LogInformation(
                "Ingestion complete finished. FileId={FileId}, Status=Processed",
                message.FileId);

        }
    }
}