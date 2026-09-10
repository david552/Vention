using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Vention.Application;

namespace Vention.API.Authorization.Authentication;

public sealed class GatewayAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly GatewayOptions _gatewayOptions;

    public GatewayAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<GatewayOptions> gatewayOptions)
        : base(options, logger, encoder)
    {
        _gatewayOptions = gatewayOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var rawUserId = Request.Headers[_gatewayOptions.UserIdHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(rawUserId)
            || !Guid.TryParse(rawUserId, out var userId)
            || userId == Guid.Empty)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}