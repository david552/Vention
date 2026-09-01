using Vention.Domain.Files;

namespace Vention.Application.Files.IntegrationEvents
{

    public sealed record FileStatusChanged(
        Guid FileId,
        Guid OrganizationId,
        Guid OwnerId,
        FileStatus Status,
        string Filename,
        DateTimeOffset ChangedAtUtc);
}