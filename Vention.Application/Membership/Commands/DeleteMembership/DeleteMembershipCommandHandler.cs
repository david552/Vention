using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Membership.Commands.DeleteMembership
{
    public sealed class DeleteMembershipCommandHandler : ICommandHandler<DeleteMembershipCommand>
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrganizationAuthorizationService _orgAuth;


        public DeleteMembershipCommandHandler(
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork,
            OrganizationAuthorizationService orgAuth)
        {
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _orgAuth = orgAuth;
        }

        public async Task Handle(DeleteMembershipCommand command, CancellationToken ct)
        {
            var membership = await _membershipRepository.GetByIdAsync(new MembershipId(command.Id), ct)
                ?? throw new NotFoundException($"Membership '{command.Id}' was not found.");

            await _orgAuth.EnsureCanRemoveMembershipAsync(command.ActingUserId, membership, ct);

            _membershipRepository.Remove(membership);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
