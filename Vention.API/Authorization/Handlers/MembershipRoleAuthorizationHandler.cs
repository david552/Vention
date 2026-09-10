using Microsoft.AspNetCore.Authorization;
using Vention.API.Authorization.Policies;
using Vention.Application.Abstractions;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using Vention.Presentation.Common.Extensions;

namespace Vention.API.Authorization.Handlers;

public sealed class MembershipRoleAuthorizationHandler
    : AuthorizationHandler<MembershipRoleRequirement>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MembershipRoleAuthorizationHandler(
        ICurrentUserService currentUser,
        IMembershipRepository membershipRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _membershipRepository = membershipRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MembershipRoleRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        if (!_currentUser.IsAuthenticated)
            return;

        if (!TryResolveOrganizationId(httpContext, requirement, out var organizationId, out var failureDetail))
        {
            httpContext.Items[AuthorizationFailureKeys.StatusCode] = StatusCodes.Status400BadRequest;
            httpContext.Items[AuthorizationFailureKeys.Detail] = failureDetail;
            return;
        }

        var membership = await _membershipRepository.GetByUserAndOrganizationAsync(
            new UserId(_currentUser.UserId),
            new OrganizationId(organizationId),
            httpContext.RequestAborted);

        if (membership is null || !MembershipRoleRules.IsAllowed(membership.Role, requirement.AllowedRoles))
            return;

        httpContext.Items["ActiveOrganizationId"] = organizationId;
        httpContext.Items["ActiveOrganizationRole"] = membership.Role;

        context.Succeed(requirement);
    }

    private static bool TryResolveOrganizationId(
        HttpContext httpContext,
        MembershipRoleRequirement requirement,
        out Guid organizationId,
        out string failureDetail)
    {
        organizationId = Guid.Empty;
        failureDetail = string.Empty;

        if (requirement.Source == OrganizationIdSource.Header)
        {
            if (httpContext.Request.TryGetOrganizationId(out organizationId))
                return true;

            failureDetail =
                $"Active organisation required. Pass '{HttpRequestTenantExtensions.OrgHeaderName}' header or 'orgId' query parameter.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(requirement.RouteParameterName))
        {
            failureDetail = "Organization route parameter is not configured.";
            return false;
        }

        if (!httpContext.Request.RouteValues.TryGetValue(requirement.RouteParameterName, out var raw)
            || raw is null
            || !Guid.TryParse(raw.ToString(), out organizationId)
            || organizationId == Guid.Empty)
        {
            failureDetail = $"Invalid or missing '{requirement.RouteParameterName}' route value.";
            return false;
        }

        return true;
    }
}