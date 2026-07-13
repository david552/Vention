namespace Vention.Application.Messages.Contracts
{
    public sealed record ChatMessageResponse(
        Guid Id,
        Guid ChatSessionId,
        Guid SenderId,
        string Content,
        DateTimeOffset CreatedAt);
}
