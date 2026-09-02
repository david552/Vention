using Microsoft.AspNetCore.SignalR;
using Vention.API.Hubs;
using Vention.Application.Abstractions;
using Vention.Application.Chats.Contracts;

namespace Vention.API.Services
{
    public sealed class SignalRNotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public SignalRNotificationPublisher(
            IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }

        private static string OrgGroup(Guid organizationId) => $"org-{organizationId}";

        public async Task NotifyJobStartedAsync(
            Guid organizationId,
            Guid fileId,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients
                .Group(OrgGroup(organizationId))
                .JobStarted(new FileJobNotification(fileId, fileName));
        }

        public async Task NotifyJobFinishedAsync(
            Guid organizationId,
            Guid fileId,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients
                .Group(OrgGroup(organizationId))
                .JobFinished(new FileJobNotification(fileId, fileName));
        }

        public async Task NotifyUserMessageAsync(
            Guid recipientUserId,
            Guid sessionId,
            Guid messageId,
            Guid senderId,
            string content,
            DateTimeOffset createdAt,
            CancellationToken cancellationToken = default)
        {
            var notification = new ChatMessageNotification(
                sessionId,
                new ChatMessageNotificationPayload(messageId, senderId, content, createdAt));

            await _hubContext.Clients
                .User(recipientUserId.ToString())
                .UserMessage(notification);
        }

        public async Task NotifyChatSessionCreatedAsync(
            Guid recipientUserId,
            ChatSessionResponse session,
            CancellationToken cancellationToken = default)
        {
            var notification = new ChatSessionCreatedNotification(
                session.Id,
                session.Participant,
                session.LastMessage,
                session.LastMessageAt,
                session.UnreadCount);

            await _hubContext.Clients
                .User(recipientUserId.ToString())
                .ChatSessionCreated(notification);
        }

        public async Task NotifyChatRenamedAsync(
           Guid organizationId,
           Guid sessionId,
           string title,
           CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients
                .Group(OrgGroup(organizationId))
                .ChatRenamed(new ChatRenamedNotification(sessionId, title));
        }
    }
}