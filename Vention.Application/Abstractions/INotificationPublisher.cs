using Vention.Application.Chats.Contracts;

namespace Vention.Application.Abstractions
{

    public interface INotificationPublisher
    {
        Task NotifyJobStartedAsync(
            Guid organizationId,
            Guid fileId,
            string fileName,
            CancellationToken cancellationToken = default);

        Task NotifyJobFinishedAsync(
            Guid organizationId,
            Guid fileId,
            string fileName,
            CancellationToken cancellationToken = default);

        Task NotifyUserMessageAsync(
            Guid recipientUserId,
            Guid sessionId,
            Guid messageId,
            Guid senderId,
            string content,
            DateTimeOffset createdAt,
            CancellationToken cancellationToken = default);

        Task NotifyChatRenamedAsync(
            Guid organizationId,
            Guid sessionId,
            string title,
            CancellationToken cancellationToken = default);

        Task NotifyChatSessionCreatedAsync(
            Guid recipientUserId,
            ChatSessionResponse session,
            CancellationToken cancellationToken = default);

    }
}