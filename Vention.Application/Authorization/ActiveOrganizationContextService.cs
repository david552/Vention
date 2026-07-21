using Vention.Application.Exceptions;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Authorization;

public sealed class ActiveOrganizationContextService
{
    private readonly IMembershipRepository _membershipRepository;

    public ActiveOrganizationContextService(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<Vention.Domain.Membership.Membership> GetMembershipOrThrowAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken ct)
    {
        var membership = await _membershipRepository.GetByUserAndOrganizationAsync(
            new UserId(userId),
            new OrganizationId(organizationId),
            ct);

        if (membership is null)
            throw new ForbiddenException("You are not a member of this organization.");

        return membership;
    }

    public async Task EnsureIsMemberAsync(Guid userId, Guid organizationId, CancellationToken ct)
    {
        await GetMembershipOrThrowAsync(userId, organizationId, ct);
    }

    public async Task EnsureHasRoleAsync(
        Guid userId,
        Guid organizationId,
        MembershipRole[] allowedRoles,
        CancellationToken ct)
    {
        var membership = await GetMembershipOrThrowAsync(userId, organizationId, ct);

        if (!MembershipRoleRules.IsAllowed(membership.Role, allowedRoles))
            throw new ForbiddenException("Insufficient permissions in this organization.");
    }
}