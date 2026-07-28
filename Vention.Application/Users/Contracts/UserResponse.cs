namespace Vention.Application.Users.Contracts
{
    public sealed record UserResponse(
        Guid Id,
        string Email,
        string Name,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<UserOrganizationMembershipResponse> Organisations);

    public sealed record UserOrganizationMembershipResponse(Guid Id, string Name, string Role);
}
