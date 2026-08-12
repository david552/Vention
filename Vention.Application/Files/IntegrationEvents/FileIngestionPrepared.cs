namespace Vention.Application.Files.IntegrationEvents
{
    public sealed record FileIngestionPrepared(
        Guid FileId,
        Guid OrganizationId,
        Guid OwnerId,
        string Filename,
        string Checksum,
        string StorageKey,
        string ContentType,
        long Size,
        DateTimeOffset PreparedAtUtc);
}