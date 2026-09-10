using Microsoft.AspNetCore.Authorization;
using Vention.Domain.Membership;

namespace Vention.API.Authorization.Policies;

public sealed class MembershipRoleRequirement : IAuthorizationRequirement
{
    public IReadOnlyList<MembershipRole> AllowedRoles { get; }
    public OrganizationIdSource Source { get; }
    public string? RouteParameterName { get; }

    public MembershipRoleRequirement(
        IEnumerable<MembershipRole> allowedRoles,
        OrganizationIdSource source,
        string? routeParameterName = null)
    {
        AllowedRoles = allowedRoles.ToArray();
        Source = source;
        RouteParameterName = routeParameterName;
    }
}