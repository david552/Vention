using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Organizations;

namespace Vention.Application.Organizations.Commands.DeleteOrganization
{
    public sealed class DeleteOrganizationCommandHandler : ICommandHandler<DeleteOrganizationCommand>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteOrganizationCommandHandler(IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork)
        {
            _organizationRepository = organizationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteOrganizationCommand command, CancellationToken ct)
        {
            var organization = await _organizationRepository.GetByIdAsync(new OrganizationId(command.Id), ct)
                ?? throw new NotFoundException($"Organization '{command.Id}' was not found.");

            organization.Delete();
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
