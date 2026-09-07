namespace Vention.Application.Messages.Contracts
{

    public sealed record ChatMessageResponse(
        Guid Id,
        Guid ChatId,
        string Content,
        Guid SenderId,
        string SenderName,
        DateTimeOffset CreatedAt,
        bool IsOwn);
}