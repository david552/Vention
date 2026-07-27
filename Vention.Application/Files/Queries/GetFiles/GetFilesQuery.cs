using Vention.Application.Files.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Files.Queries.GetFiles
{
    public sealed record GetFilesQuery(
        Guid OrganizationId,
        int Limit) : IQuery<IReadOnlyList<FileResponse>>;
}