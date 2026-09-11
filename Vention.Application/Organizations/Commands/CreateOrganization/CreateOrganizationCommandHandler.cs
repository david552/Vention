using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
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
        private readonly UserAuthorizationService _userAuthorizationService;

        public CreateOrganizationCommandHandler(
            IOrganizationRepository organizationRepository,
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork, 
            UserAuthorizationService userAuthorizationService)
        {
            _organizationRepository = organizationRepository;
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _userAuthorizationService = userAuthorizationService;
        }

        public async Task<OrganizationResponse> Handle(CreateOrganizationCommand command, CancellationToken ct)
        {
            if (!await _userAuthorizationService.IsActive(command.UserId,ct))
            {
                throw new NotFoundException("User Not Found");
            }

            var organization = Organization.Create(command.Name);
            _organizationRepository.Add(organization);

            var ownerMembership = DomainMembership.Create(new UserId(command.UserId), organization.Id, MembershipRole.Owner);
            _membershipRepository.Add(ownerMembership);

            await _unitOfWork.SaveChangesAsync(ct);

            return organization.Adapt<OrganizationResponse>();
        }
    }
}
