using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using DomainMembership = Vention.Domain.Membership.Membership;

namespace Vention.Application.Membership.Commands.CreateMembership
{
    public sealed class CreateMembershipCommandHandler : ICommandHandler<CreateMembershipCommand, MembershipResponse>
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateMembershipCommandHandler(
            IMembershipRepository membershipRepository,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            IUnitOfWork unitOfWork)
        {
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MembershipResponse> Handle(CreateMembershipCommand command, CancellationToken ct)
        {
            var userId = new UserId(command.UserId);
            var organizationId = new OrganizationId(command.OrganizationId);

            if (!Enum.TryParse<MembershipRole>(command.Role, ignoreCase: true, out var role))
                throw new ArgumentException($"'{command.Role}' is not a valid membership role.", nameof(command.Role));

            if (!await _userRepository.ExistsByIdAsync(userId, ct))
                throw new NotFoundException($"User '{command.UserId}' was not found.");

            if (!await _organizationRepository.ExistsByIdAsync(organizationId, ct))
                throw new NotFoundException($"Organization '{command.OrganizationId}' was not found.");

            if (await _membershipRepository.ExistsAsync(userId, organizationId, ct))
                throw new InvalidOperationException("This user is already a member of this organization.");

            var membership = DomainMembership.Create(userId, organizationId, role);

            _membershipRepository.Add(membership);
            await _unitOfWork.SaveChangesAsync(ct);

            return membership.Adapt<MembershipResponse>();
        }
    }
}
