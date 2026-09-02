namespace Vention.Application.Chats.Contracts
{

    public sealed record ChatParticipantResponse(Guid Id, string Name);

    public sealed record ChatSessionResponse(
        Guid Id,
        ChatParticipantResponse Participant,
        string LastMessage,
        DateTimeOffset LastMessageAt,
        int UnreadCount);
}