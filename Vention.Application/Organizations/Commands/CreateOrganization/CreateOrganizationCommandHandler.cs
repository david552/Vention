using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;
using Vention.Domain.Organizations;

namespace Vention.Application.Organizations.Commands.CreateOrganization
{
    public sealed class CreateOrganizationCommandHandler : ICommandHandler<CreateOrganizationCommand, OrganizationResponse>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrganizationCommandHandler(IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork)
        {
            _organizationRepository = organizationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrganizationResponse> Handle(CreateOrganizationCommand command, CancellationToken ct)
        {
            var organization = Organization.Create(command.Name);

            _organizationRepository.Add(organization);
            await _unitOfWork.SaveChangesAsync(ct);

            return organization.Adapt<OrganizationResponse>();
        }
    }
}
