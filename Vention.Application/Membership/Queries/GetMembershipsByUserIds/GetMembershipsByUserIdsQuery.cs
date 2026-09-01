using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Membership.Queries.GetMembershipsByUserIds
{
    public sealed record GetMembershipsByUserIdsQuery(IReadOnlyList<Guid> UserIds, Guid ActingUserId)
        : IQuery<IReadOnlyList<MembershipResponse>>;
}