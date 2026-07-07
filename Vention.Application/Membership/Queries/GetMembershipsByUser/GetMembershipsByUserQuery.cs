using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Membership.Queries.GetMembershipsByUser
{
    public sealed record GetMembershipsByUserQuery(Guid UserId) : IQuery<IReadOnlyList<MembershipResponse>>;

}
