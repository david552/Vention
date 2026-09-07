using Vention.Application.Chats.Contracts;

namespace Vention.Application.Abstractions
{

    public interface INotificationClient
    {
        Task JobStarted(FileJobNotification notification);
        Task JobFinished(FileJobNotification notification);

        Task UserConnected(string userId);
        Task UserDisconnected(string userId);
        Task OnlineUsersSnapshot(IReadOnlyList<string> userIds);

        Task UserMessage(ChatMessageNotification notification);
        Task ChatRenamed(ChatRenamedNotification notification);
        Task ChatSessionCreated(ChatSessionCreatedNotification notification);
    }

    public sealed record FileJobNotification(Guid FileId, string FileName);

    public sealed record ChatMessageNotification(
        Guid SessionId,
        ChatMessageNotificationPayload Message);

    public sealed record ChatMessageNotificationPayload(
        Guid Id,
        Guid SenderId,
        string Content,
        DateTimeOffset CreatedAt);

    public sealed record ChatSessionCreatedNotification(
        Guid SessionId,
        ChatParticipantResponse Initiator,
        string LastMessage,
        DateTimeOffset LastMessageAt,
        int UnreadCount);

    public sealed record ChatRenamedNotification(Guid SessionId, string Title);
}
