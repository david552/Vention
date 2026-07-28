using Vention.Application.Exceptions;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Authorization
{
    public sealed class OrganizationAuthorizationService
    {
        private readonly IMembershipRepository _membershipRepository;

        public OrganizationAuthorizationService(IMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

        public async Task EnsureIsOrganizationMemberAsync(
            Guid actingUserId,
            Guid organizationId,
            CancellationToken ct)
        {
            var membership = await _membershipRepository.GetByUserAndOrganizationAsync(
                new UserId(actingUserId),
                new OrganizationId(organizationId),
                ct);

            if (membership is null)
                throw new ForbiddenException("You are not a member of this organization.");
        }

        public async Task EnsureCanManageMembersAsync(
            Guid actingUserId,
            Guid organizationId,
            CancellationToken ct)
        {
            var actingMembership = await GetActingMembershipOrThrowAsync(actingUserId, organizationId, ct);

            if (!MembershipRoleRules.IsOwnerOrAdmin(actingMembership.Role))
                throw new ForbiddenException("Only an Owner or Admin of the organization can manage members.");
        }

        public async Task EnsureCanAssignRoleAsync(
            Guid actingUserId,
            Guid organizationId,
            MembershipRole targetRole,
            CancellationToken ct)
        {
            var actingMembership = await GetActingMembershipOrThrowAsync(actingUserId, organizationId, ct);

            if (!MembershipRoleRules.CanAssign(actingMembership.Role, targetRole))
                throw new ForbiddenException(
                    $"You are not allowed to assign the '{targetRole}' role.");
        }

        public async Task EnsureCanRemoveMembershipAsync(
            Guid actingUserId,
            Vention.Domain.Membership.Membership targetMembership,
            CancellationToken ct)
        {
            var actingMembership = await GetActingMembershipOrThrowAsync(
                actingUserId,
                targetMembership.OrganizationId.Value,
                ct);

            if (!MembershipRoleRules.CanRemove(actingMembership.Role, targetMembership.Role))
                throw new ForbiddenException("You are not allowed to remove this membership.");

            if (targetMembership.Role == MembershipRole.Owner)
                await EnsureNotLastOwnerAsync(targetMembership, ct);
        }

        public async Task EnsureCanChangeMembershipRoleAsync(
            Guid actingUserId,
            Vention.Domain.Membership.Membership targetMembership,
            MembershipRole newRole,
            CancellationToken ct)
        {
            await EnsureCanAssignRoleAsync(
                actingUserId,
                targetMembership.OrganizationId.Value,
                newRole,
                ct);

            if (targetMembership.Role == MembershipRole.Owner && newRole != MembershipRole.Owner)
                await EnsureNotLastOwnerAsync(targetMembership, ct);
        }

        private async Task<Vention.Domain.Membership.Membership> GetActingMembershipOrThrowAsync(
            Guid actingUserId,
            Guid organizationId,
            CancellationToken ct)
        {
            var actingMembership = await _membershipRepository.GetByUserAndOrganizationAsync(
                new UserId(actingUserId),
                new OrganizationId(organizationId),
                ct);

            if (actingMembership is null)
                throw new ForbiddenException("You are not a member of this organization.");

            return actingMembership;
        }

        private async Task EnsureNotLastOwnerAsync(Vention.Domain.Membership.Membership targetMembership, CancellationToken ct)
        {
            var members = await _membershipRepository.GetByOrganizationIdAsync(
                targetMembership.OrganizationId,
                ct);

            var ownerCount = members.Count(m => m.Role == MembershipRole.Owner);
            if (ownerCount <= 1)
                throw new ForbiddenException("Cannot remove or demote the last Owner of the organization.");
        }
    }
}