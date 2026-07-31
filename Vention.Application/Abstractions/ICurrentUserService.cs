namespace Vention.Application.Abstractions;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
}