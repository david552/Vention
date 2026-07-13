using Vention.Application.Messaging;

namespace Vention.Application.Membership.Commands.DeleteMembership
{
    public sealed record DeleteMembershipCommand(Guid Id) : ICommand;

}
