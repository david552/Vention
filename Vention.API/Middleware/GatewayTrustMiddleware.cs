using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Vention.Application;

namespace Vention.API.Middleware;

public sealed class GatewayTrustMiddleware
{
    private readonly RequestDelegate _next;

    public GatewayTrustMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IOptions<GatewayOptions> options)
    {
        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        if (allowAnonymous)
        {
            await _next(context);
            return;
        }

        var settings = options.Value;
        var providedSecret = context.Request.Headers[settings.GatewaySecretHeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(providedSecret) || !SecureEquals(providedSecret, settings.SharedSecret))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized",
                detail = "Request must come from the trusted API Gateway."
            });
            return;
        }

        var userIdHeader = context.Request.Headers[settings.UserIdHeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userIdHeader)
            || !Guid.TryParse(userIdHeader, out var userId)
            || userId == Guid.Empty)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized",
                detail = $"Missing or invalid '{settings.UserIdHeaderName}' header."
            });
            return;
        }

        await _next(context);
    }

    private static bool SecureEquals(string provided, string expected)
    {
        var a = Encoding.UTF8.GetBytes(provided);
        var b = Encoding.UTF8.GetBytes(expected);

        if (a.Length != b.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}