using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Common;
using Vention.Application.Files.Contracts;
using Vention.Application.Files.IntegrationEvents;
using Vention.Application.Messaging;
using Vention.Domain.Files;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Files.Commands.UploadFile
{
    public sealed class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, FileResponse>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;


        public UploadFileCommandHandler(
            IStoredFileRepository storedFileRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork,
            IIntegrationEventPublisher integrationEventPublisher)
        {
            _storedFileRepository = storedFileRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<FileResponse> Handle(UploadFileCommand command, CancellationToken ct)
        {

            var filename = FileUploadRules.SanitizeFilename(command.Filename);

            var header = new byte[FileUploadRules.SignatureLength];
            var headerLength = await ReadHeaderAsync(command.Content, header, ct);

            if (headerLength == 0)
                throw new ArgumentException("The uploaded file is empty.", nameof(command.Content));

            if (!FileUploadRules.MatchesSignature(command.ContentType, header.AsSpan(0, headerLength)))
                throw new ArgumentException(
                    $"The file content does not match the declared content type '{command.ContentType}'.",
                    nameof(command.ContentType));

            var extension = FileUploadRules.GetExtensionFor(command.ContentType);

            await using var content = new PrefixedStream(header, headerLength, command.Content);

            var stored = await _fileStorageService.SaveAsync(
                content,
                command.OrganizationId,
                extension,
                FileUploadRules.MaxFileSizeBytes,
                ct);

            var organizationId = new OrganizationId(command.OrganizationId);

            var existing = await _storedFileRepository.GetByOrganizationAndChecksumAsync(
                organizationId, stored.Checksum, ct);

            if (existing is not null)
                return existing.Adapt<FileResponse>();

            var storedFile = StoredFile.Create(
                filename,
                stored.Size,
                command.ContentType.Trim(),
                stored.Checksum,
                stored.StorageKey,
                organizationId,
                new UserId(command.ActingUserId));

            _storedFileRepository.Add(storedFile);

            await _integrationEventPublisher.PublishAsync(
                new FileIngestionRequested(
                    storedFile.Id.Value,
                    command.OrganizationId,
                    command.ActingUserId,
                    storedFile.Filename,
                    storedFile.Checksum,
                    storedFile.StorageKey,
                    storedFile.ContentType,
                    storedFile.Size,
                    DateTimeOffset.UtcNow),
                ct);

            await _integrationEventPublisher.PublishAsync(
                new FileStatusChanged(
                    storedFile.Id.Value,
                    command.OrganizationId,
                    command.ActingUserId,
                    FileStatus.Processing,
                    storedFile.Filename,
                    DateTimeOffset.UtcNow),
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return storedFile.Adapt<FileResponse>();
        }

        private static async Task<int> ReadHeaderAsync(Stream content, byte[] header, CancellationToken ct)
        {
            var total = 0;
            while (total < header.Length)
            {
                var read = await content.ReadAsync(header.AsMemory(total, header.Length - total), ct);
                if (read == 0)
                    break;
                total += read;
            }

            return total;
        }
    }
}