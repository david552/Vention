namespace Vention.Application.Users.Contracts
{
    public sealed record UserResponse(Guid Id, string Email, string Name, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

}
