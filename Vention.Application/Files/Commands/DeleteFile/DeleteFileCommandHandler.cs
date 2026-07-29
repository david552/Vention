using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Files;

namespace Vention.Application.Files.Commands.DeleteFile
{
    public sealed class DeleteFileCommandHandler : ICommandHandler<DeleteFileCommand>
    {
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteFileCommandHandler(
            IStoredFileRepository storedFileRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork)
        {
            _storedFileRepository = storedFileRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteFileCommand command, CancellationToken ct)
        {
            var storedFile = await _storedFileRepository.GetByIdAsync(new StoredFileId(command.FileId), ct)
                ?? throw new NotFoundException($"File '{command.FileId}' was not found.");

            if (storedFile.OrganizationId.Value != command.OrganizationId)
                throw new NotFoundException($"File '{command.FileId}' was not found.");

            await _fileStorageService.DeleteAsync(storedFile.StorageKey, ct);

            _storedFileRepository.Remove(storedFile);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}