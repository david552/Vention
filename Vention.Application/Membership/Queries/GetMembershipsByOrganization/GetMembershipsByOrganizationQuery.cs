using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Membership.Queries.GetMembershipsByOrganization
{
    public sealed record GetMembershipsByOrganizationQuery(Guid OrganizationId) : IQuery<IReadOnlyList<MembershipResponse>>;

}
