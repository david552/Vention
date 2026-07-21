using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Membership.Commands.CreateMembership
{
   public sealed record CreateMembershipCommand(Guid UserId, Guid OrganizationId, string Role, Guid ActingUserId) : ICommand<MembershipResponse>;

}
