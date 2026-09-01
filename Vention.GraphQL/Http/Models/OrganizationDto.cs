namespace Vention.GraphQL.Http.Models
{
    public sealed class OrganizationDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }

    public sealed class CreateOrganizationRequestDto
    {
        public string Name { get; init; } = string.Empty;
    }

    public sealed class UpdateOrganizationRequestDto
    {
        public string Name { get; init; } = string.Empty;
    }
}