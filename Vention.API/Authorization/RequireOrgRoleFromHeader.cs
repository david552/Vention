using Microsoft.AspNetCore.Authorization;
using Vention.API.Authorization.Policies;
using Vention.Domain.Membership;

namespace Vention.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireOrgRoleFromHeader : AuthorizeAttribute, IAuthorizationRequirementData
{
    public MembershipRole[] AllowedRoles { get; }

    public RequireOrgRoleFromHeader(params MembershipRole[] allowedRoles)
    {
        AllowedRoles = allowedRoles.Length > 0
            ? allowedRoles
            :
            [
                MembershipRole.Owner,
                MembershipRole.Admin,
                MembershipRole.Editor,
                MembershipRole.Member,
                MembershipRole.Viewer
            ];
    }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new MembershipRoleRequirement(
            AllowedRoles,
            OrganizationIdSource.Header);
    }
}