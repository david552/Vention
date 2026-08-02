using Vention.Domain.Common;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Domain.Files
{
    public sealed class StoredFile : AggregateRoot<StoredFileId>
    {
        public string Filename { get; private set; } = null!;
        public long Size { get; private set; }
        public FileStatus Status { get; private set; }
        public string ContentType { get; private set; } = null!;
        public string Checksum { get; private set; } = null!;
        public string StorageKey { get; private set; } = null!;
        public OrganizationId OrganizationId { get; private set; }
        public UserId OwnerId { get; private set; }
        public string? Application { get; private set; }
        public string? ProcessingError { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private StoredFile() { }

        private StoredFile(
            StoredFileId id,
            string filename,
            long size,
            string contentType,
            string checksum,
            string storageKey,
            OrganizationId organizationId,
            UserId ownerId) : base(id)
        {
            Filename = filename;
            Size = size;
            Status = FileStatus.Processing;
            ContentType = contentType;
            Checksum = checksum;
            StorageKey = storageKey;
            OrganizationId = organizationId;
            OwnerId = ownerId;
            Application = null;
            ProcessingError = null;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static StoredFile Create(
            string filename,
            long size,
            string contentType,
            string checksum,
            string storageKey,
            OrganizationId organizationId,
            UserId ownerId)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be empty.", nameof(filename));


            if (filename.Contains('/') || filename.Contains('\\') || filename.Contains(".."))
                throw new ArgumentException("Filename cannot contain path segments.", nameof(filename));

            if (size <= 0)
                throw new ArgumentException("Size must be greater than zero.", nameof(size));

            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type cannot be empty.", nameof(contentType));

            if (string.IsNullOrWhiteSpace(checksum))
                throw new ArgumentException("Checksum cannot be empty.", nameof(checksum));

            if (string.IsNullOrWhiteSpace(storageKey))
                throw new ArgumentException("Storage key cannot be empty.", nameof(storageKey));

            if (organizationId.Value == Guid.Empty)
                throw new ArgumentException("OrganizationId cannot be empty.", nameof(organizationId));

            if (ownerId.Value == Guid.Empty)
                throw new ArgumentException("OwnerId cannot be empty.", nameof(ownerId));

            return new StoredFile(
                new StoredFileId(Guid.NewGuid()),
                filename.Trim(),
                size,
                contentType.Trim(),
                checksum,
                storageKey,
                organizationId,
                ownerId);
        }

        public void MarkProcessed()
        {
            Status = FileStatus.Processed;
            ProcessingError = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }


        public void MarkFailed(string processingError)
        {
            if (string.IsNullOrWhiteSpace(processingError))
                throw new ArgumentException("Processing error cannot be empty.", nameof(processingError));

            Status = FileStatus.Error;
            ProcessingError = processingError.Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkProcessing()
        {
            Status = FileStatus.Processing;
            ProcessingError = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public record StoredFileId(Guid Value);
}
