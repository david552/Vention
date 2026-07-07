using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;

namespace Vention.Application.Organizations.Queries.GetOrganizations
{
    public sealed record GetOrganizationsQuery : IQuery<IReadOnlyList<OrganizationResponse>>;

}
