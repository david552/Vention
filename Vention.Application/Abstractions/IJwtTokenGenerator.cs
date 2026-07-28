using Vention.Domain.Users;

namespace Vention.Application.Abstractions
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
    }
}