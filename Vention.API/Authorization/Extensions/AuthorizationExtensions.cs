using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Vention.API.Authorization.Authentication;
using Vention.API.Authorization.Handlers;

namespace Vention.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddVentionAuthorization(this IServiceCollection services)
    {
        services.AddAuthentication(GatewayAuthenticationDefaults.Scheme)
            .AddScheme<AuthenticationSchemeOptions, GatewayAuthenticationHandler>(
                GatewayAuthenticationDefaults.Scheme,
                null);

        services.AddAuthorization();

        services.AddScoped<IAuthorizationHandler, MembershipRoleAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, VentionAuthorizationMiddlewareResultHandler>();

        return services;
    }
}