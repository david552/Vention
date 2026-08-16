namespace Vention.GraphQL.Http.Models
{

    public sealed class UserDto
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
        public IReadOnlyList<UserOrganizationMembershipDto> Organisations { get; init; }
            = Array.Empty<UserOrganizationMembershipDto>();
    }

    public sealed class UserOrganizationMembershipDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }

    public sealed class CreateUserRequestDto
    {
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    public sealed class UpdateUserRequestDto
    {
        public string DisplayName { get; init; } = string.Empty;
    }
}