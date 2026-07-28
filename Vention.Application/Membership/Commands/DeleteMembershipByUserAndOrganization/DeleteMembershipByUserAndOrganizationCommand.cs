using Vention.Application.Messaging;

namespace Vention.Application.Membership.Commands.DeleteMembershipByUserAndOrganization
{
    public sealed record DeleteMembershipByUserAndOrganizationCommand(Guid UserId, Guid OrganizationId, Guid ActingUserId) : ICommand;
}
