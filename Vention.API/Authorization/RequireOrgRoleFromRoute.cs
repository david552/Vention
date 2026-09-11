using Microsoft.AspNetCore.Authorization;
using Vention.API.Authorization.Policies;
using Vention.Domain.Membership;

namespace Vention.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireOrgRoleFromRoute : AuthorizeAttribute, IAuthorizationRequirementData
{
    public RequireOrgRoleFromRoute(string organizationIdRouteKey, params MembershipRole[] allowedRoles)
    {
        OrganizationIdRouteKey = organizationIdRouteKey;
        AllowedRoles = allowedRoles;
    }

    public string OrganizationIdRouteKey { get; }
    public MembershipRole[] AllowedRoles { get; }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new MembershipRoleRequirement(
            AllowedRoles,
            OrganizationIdSource.Route,
            OrganizationIdRouteKey);
    }
}