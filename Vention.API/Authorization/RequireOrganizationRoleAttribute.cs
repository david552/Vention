using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Vention.Application.Abstractions;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.API.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class RequireOrganizationRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _organizationIdRouteKey;
        private readonly MembershipRole[] _allowedRoles;

        public RequireOrganizationRoleAttribute(string organizationIdRouteKey, params MembershipRole[] allowedRoles)
        {
            _organizationIdRouteKey = organizationIdRouteKey;
            _allowedRoles = allowedRoles;
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

            if (!context.RouteData.Values.TryGetValue(_organizationIdRouteKey, out var raw)
                || raw is null
                || !Guid.TryParse(raw.ToString(), out var organizationId))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.BadRequestResult();
                return;
            }

            var membershipRepository = context.HttpContext.RequestServices.GetRequiredService<IMembershipRepository>();
            var membership = await membershipRepository.GetByUserAndOrganizationAsync(
                new UserId(userId), new OrganizationId(organizationId), context.HttpContext.RequestAborted);

            if (membership == null || !MembershipRoleRules.IsAllowed(membership.Role, _allowedRoles))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
