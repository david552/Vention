using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;
using Vention.Domain.Organizations;

namespace Vention.Application.Organizations.Commands.UpdateOrganization
{
    public sealed class UpdateOrganizationCommandHandler : ICommandHandler<UpdateOrganizationCommand, OrganizationResponse>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrganizationCommandHandler(IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork)
        {
            _organizationRepository = organizationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrganizationResponse> Handle(UpdateOrganizationCommand command, CancellationToken ct)
        {
            var organization = await _organizationRepository.GetByIdAsync(new OrganizationId(command.Id), ct)
                ?? throw new NotFoundException($"Organization '{command.Id}' was not found.");

            organization.Rename(command.Name);
            await _unitOfWork.SaveChangesAsync(ct);

            return organization.Adapt<OrganizationResponse>();
        }
    }
}
