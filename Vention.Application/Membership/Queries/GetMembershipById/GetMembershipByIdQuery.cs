using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Membership.Queries.GetMembershipById
{
    public sealed record GetMembershipByIdQuery(Guid Id, Guid ActingUserId) : IQuery<MembershipResponse>;

}
