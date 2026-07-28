using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using DomainMembership = Vention.Domain.Membership.Membership;

namespace Vention.Application.Organizations.Commands.CreateOrganization
{
    public sealed class CreateOrganizationCommandHandler : ICommandHandler<CreateOrganizationCommand, OrganizationResponse>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrganizationCommandHandler(
            IOrganizationRepository organizationRepository,
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork)
        {
            _organizationRepository = organizationRepository;
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrganizationResponse> Handle(CreateOrganizationCommand command, CancellationToken ct)
        {
            var organization = Organization.Create(command.Name);
            _organizationRepository.Add(organization);

            var ownerMembership = DomainMembership.Create(new UserId(command.UserId), organization.Id, MembershipRole.Owner);
            _membershipRepository.Add(ownerMembership);

            await _unitOfWork.SaveChangesAsync(ct);

            return organization.Adapt<OrganizationResponse>();
        }
    }
}
