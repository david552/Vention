using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Users;

namespace Vention.Application.Membership.Commands.ChangeMembershipRole
{
    public sealed class ChangeMembershipRoleCommandHandler : ICommandHandler<ChangeMembershipRoleCommand, MembershipResponse>
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrganizationAuthorizationService _orgAuth;


        public ChangeMembershipRoleCommandHandler(
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork,
            OrganizationAuthorizationService orgAuth)
        {
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _orgAuth = orgAuth;

        }

        public async Task<MembershipResponse> Handle(ChangeMembershipRoleCommand command, CancellationToken ct)
        {
            var membership = await _membershipRepository.GetByIdAsync(new MembershipId(command.Id), ct)
                ?? throw new NotFoundException($"Membership '{command.Id}' was not found.");

            if (!Enum.TryParse<MembershipRole>(command.Role, ignoreCase: true, out var role))
                throw new ArgumentException($"'{command.Role}' is not a valid membership role.", nameof(command.Role));

            await _orgAuth.EnsureCanChangeMembershipRoleAsync(command.ActingUserId, membership, role, ct);

            membership.ChangeRole(role);
            await _unitOfWork.SaveChangesAsync(ct);

            return membership.Adapt<MembershipResponse>();
        }
    }
}
