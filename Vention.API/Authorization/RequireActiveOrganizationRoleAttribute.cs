using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Vention.Presentation.Common.Extensions;
using Vention.Application.Abstractions;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.API.Authorization;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireActiveOrganizationRoleAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly MembershipRole[] _allowedRoles;

    public RequireActiveOrganizationRoleAttribute(params MembershipRole[] allowedRoles)
    {
        _allowedRoles = allowedRoles.Length > 0
            ? allowedRoles
            : new[]
            {
                MembershipRole.Owner,
                MembershipRole.Admin,
                MembershipRole.Editor,
                MembershipRole.Member,
                MembershipRole.Viewer
            };
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        if (!currentUser.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = currentUser.UserId;

        if (!context.HttpContext.Request.TryGetOrganizationId(out var organizationId))
        {
            context.Result = new BadRequestObjectResult(new
            {
                title = "Active organisation required",
                detail = $"Pass '{HttpRequestTenantExtensions.OrgHeaderName}' header or 'orgId' query parameter."
            });
            return;
        }

        var membershipRepository = context.HttpContext.RequestServices
            .GetRequiredService<IMembershipRepository>();

        var membership = await membershipRepository.GetByUserAndOrganizationAsync(
            new UserId(userId),
            new OrganizationId(organizationId),
            context.HttpContext.RequestAborted);

        if (membership is null || !MembershipRoleRules.IsAllowed(membership.Role, _allowedRoles))
        {
            context.Result = new ForbidResult();
            return;
        }

        context.HttpContext.Items["ActiveOrganizationId"] = organizationId;
        context.HttpContext.Items["ActiveOrganizationRole"] = membership.Role;
    }
}