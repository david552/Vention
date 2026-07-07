namespace Vention.Application.Chats.Contracts
{
    public sealed record ChatSessionMemberResponse(
        Guid Id,
        Guid ChatSessionId,
        Guid UserId,
        DateTimeOffset JoinedAt);
}
