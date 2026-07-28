using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Membership.Commands.ChangeMembershipRole
{
    public sealed record ChangeMembershipRoleCommand(Guid Id, string Role, Guid ActingUserId) : ICommand<MembershipResponse>;

}
