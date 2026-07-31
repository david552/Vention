using Microsoft.Extensions.Options;
using Vention.Application;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;

namespace Vention.API.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GatewayOptions _options;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IOptions<GatewayOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    public bool IsAuthenticated
    {
        get
        {
            var raw = GetRawUserId();
            return !string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out var id) && id != Guid.Empty;
        }
    }

    public Guid UserId
    {
        get
        {
            var raw = GetRawUserId();
            if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var userId) || userId == Guid.Empty)
                throw new UnauthorizedException("Unable to resolve the current user from gateway headers.");

            return userId;
        }
    }

    private string? GetRawUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context.");

        return httpContext.Request.Headers[_options.UserIdHeaderName].FirstOrDefault();
    }
}