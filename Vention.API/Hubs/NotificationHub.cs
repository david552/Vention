using Microsoft.AspNetCore.SignalR;
using Vention.Application.Abstractions;

namespace Vention.API.Hubs
{

    public sealed class NotificationHub : Hub<INotificationClient>
    {
        public const string Route = "/hubs/notifications";

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}