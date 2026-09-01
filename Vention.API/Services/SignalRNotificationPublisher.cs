using Microsoft.AspNetCore.SignalR;
using Vention.API.Hubs;
using Vention.Application.Abstractions;

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
    }
}