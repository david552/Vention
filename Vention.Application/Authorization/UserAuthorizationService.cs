using Vention.Application.Exceptions;
using Vention.Domain.Membership;
using Vention.Domain.Users;

namespace Vention.Application.Authorization
{
    public sealed class UserAuthorizationService
    {
        private readonly IMembershipRepository _membershipRepository;
        public UserAuthorizationService(IMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }
        public async Task EnsureCanManageUserAsync(Guid targetUserId, Guid actingUserId, CancellationToken ct)
        {
            if (targetUserId == actingUserId)
                return;

            var targetMemberships = await _membershipRepository.GetByUserIdAsync(new UserId(targetUserId), ct);

            foreach (var membership in targetMemberships)
            {
                var actingMembership = await _membershipRepository.GetByUserAndOrganizationAsync(
                    new UserId(actingUserId),
                    membership.OrganizationId,
                    ct);

                if (actingMembership is not null && MembershipRoleRules.IsOwnerOrAdmin(actingMembership.Role))
                    return;
            }

            throw new ForbiddenException("You are not allowed to manage this user.");
        }

        public async Task EnsureCanViewUserAsync(Guid targetUserId, Guid actingUserId, CancellationToken ct)
        {
            if (targetUserId == actingUserId)
                return;

            var actingMemberships = await _membershipRepository.GetByUserIdAsync(new UserId(actingUserId), ct);
            if (actingMemberships.Count == 0)
                throw new ForbiddenException("You are not allowed to view this user.");

            var targetMemberships = await _membershipRepository.GetByUserIdAsync(new UserId(targetUserId), ct);

            var actingOrgIds = actingMemberships.Select(m => m.OrganizationId.Value).ToHashSet();
            if (targetMemberships.Any(m => actingOrgIds.Contains(m.OrganizationId.Value)))
                return;


            if (targetMemberships.Count == 0 &&
                actingMemberships.Any(m => MembershipRoleRules.IsOwnerOrAdmin(m.Role)))
                return;


            throw new ForbiddenException("You are not allowed to view this user.");
        }
    }
}
