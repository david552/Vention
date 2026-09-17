namespace Vention.Application.Abstractions.Auth;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
}