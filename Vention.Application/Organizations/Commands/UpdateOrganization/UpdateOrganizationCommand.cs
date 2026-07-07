using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;

namespace Vention.Application.Organizations.Commands.UpdateOrganization
{
    public sealed record UpdateOrganizationCommand(Guid Id, string Name) : ICommand<OrganizationResponse>;
}
