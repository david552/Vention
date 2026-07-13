namespace Vention.Application.Organizations.Contracts
{
    public sealed record OrganizationResponse(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

}
