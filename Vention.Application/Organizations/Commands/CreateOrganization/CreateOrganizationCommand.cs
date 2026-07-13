using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;

namespace Vention.Application.Organizations.Commands.CreateOrganization
{
    public sealed record CreateOrganizationCommand(string Name) : ICommand<OrganizationResponse>;

}
