using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;

namespace Vention.Application.Organizations.Queries.GetOrganizationById
{
    public sealed record GetOrganizationByIdQuery(Guid Id) : IQuery<OrganizationResponse>;

}
