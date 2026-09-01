using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;

namespace Vention.Application.Organizations.Queries.GetOrganizationsByIds
{

    public sealed record GetOrganizationsByIdsQuery(IReadOnlyList<Guid> Ids, Guid ActingUserId)
        : IQuery<IReadOnlyList<OrganizationResponse>>;
}