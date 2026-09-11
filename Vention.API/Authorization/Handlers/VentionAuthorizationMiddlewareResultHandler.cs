using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

using Vention.API.Authorization.Policies;

namespace Vention.API.Authorization.Handlers;

public sealed class VentionAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        if (context.Items.TryGetValue(AuthorizationFailureKeys.StatusCode, out var statusObj)
            && statusObj is int statusCode)
        {
            var detail = context.Items.TryGetValue(AuthorizationFailureKeys.Detail, out var detailObj)
                ? detailObj?.ToString()
                : "Authorization failed.";

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                status = statusCode,
                title = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Bad Request",
                    StatusCodes.Status401Unauthorized => "Unauthorized",
                    _ => "Forbidden"
                },
                detail,
                instance = context.Request.Path
            });

            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}