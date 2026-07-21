using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Membership.Commands.DeleteMembershipByUserAndOrganization
{
    public sealed class DeleteMembershipByUserAndOrganizationCommandHandler : ICommandHandler<DeleteMembershipByUserAndOrganizationCommand>
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrganizationAuthorizationService _orgAuth;


        public DeleteMembershipByUserAndOrganizationCommandHandler(
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork,
            OrganizationAuthorizationService orgAuth)
        {
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _orgAuth = orgAuth; 
        }

        public async Task Handle(DeleteMembershipByUserAndOrganizationCommand command, CancellationToken ct)
        {
            var organizationId = new OrganizationId(command.OrganizationId);

            var membership = await _membershipRepository.GetByUserAndOrganizationAsync(
                new UserId(command.UserId), organizationId, ct)
                ?? throw new NotFoundException(
                    $"Membership for user '{command.UserId}' in organization '{command.OrganizationId}' was not found.");

            await _orgAuth.EnsureCanRemoveMembershipAsync(command.ActingUserId, membership, ct);

            _membershipRepository.Remove(membership);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
