using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Files.Contracts;
using Vention.Application.Files.IntegrationEvents;
using Vention.Application.Messaging;
using Vention.Domain.Files;

namespace Vention.Application.Files.Commands.ProcessFile
{
    public sealed class ProcessFileCommandHandler : ICommandHandler<ProcessFileCommand, FileResponse>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;

        public ProcessFileCommandHandler(
            IStoredFileRepository storedFileRepository,
            IUnitOfWork unitOfWork,
            IIntegrationEventPublisher integrationEventPublisher)
        {
            _storedFileRepository = storedFileRepository;
            _unitOfWork = unitOfWork;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<FileResponse> Handle(ProcessFileCommand command, CancellationToken ct)
        {
            var storedFile = await _storedFileRepository.GetByIdAsync(new StoredFileId(command.FileId), ct)
                ?? throw new NotFoundException($"File '{command.FileId}' was not found.");

            if (storedFile.OrganizationId.Value != command.OrganizationId)
                throw new NotFoundException($"File '{command.FileId}' was not found.");

            storedFile.MarkProcessing();

            await _integrationEventPublisher.PublishAsync(
                new FileIngestionRequested(
                    storedFile.Id.Value,
                    storedFile.OrganizationId.Value,
                    storedFile.OwnerId.Value,
                    storedFile.Filename,
                    storedFile.Checksum,
                    storedFile.StorageKey,
                    storedFile.ContentType,
                    storedFile.Size,
                    DateTimeOffset.UtcNow),
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return storedFile.Adapt<FileResponse>();
        }
    }
}