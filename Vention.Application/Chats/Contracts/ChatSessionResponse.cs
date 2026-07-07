namespace Vention.Application.Chats.Contracts
{
    public sealed record ChatSessionResponse(Guid Id, Guid OrganizationId, Guid CreatedByUserId, string Title, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

}
