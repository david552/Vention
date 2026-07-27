namespace Vention.Application.Files.Contracts
{
    public sealed record FileResponse(
       Guid Id,
       string Filename,
       long Size,
       string Status,
       string ContentType,
       string Checksum,
       string StorageKey,
       Guid OrganisationId,
       Guid OwnerId,
       string? Application,
       string? ProcessingError,
       DateTimeOffset CreatedAt,
       DateTimeOffset UpdatedAt);
}
