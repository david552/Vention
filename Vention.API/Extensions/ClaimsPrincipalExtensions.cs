using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Vention.Application.Exceptions;

namespace Vention.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (value is null || !Guid.TryParse(value, out var userId))
                throw new UnauthorizedException("Unable to resolve the current user from the access token.");

            return userId;
        }
    }
}
