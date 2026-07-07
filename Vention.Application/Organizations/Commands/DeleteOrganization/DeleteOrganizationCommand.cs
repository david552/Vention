using Vention.Application.Messaging;

namespace Vention.Application.Organizations.Commands.DeleteOrganization
{
    public sealed record DeleteOrganizationCommand(Guid Id) : ICommand;

}
