using Microsoft.AspNetCore.SignalR;
using Vention.Application.Abstractions;
using Vention.Application.Messaging;
using Vention.Application.Users;
using Vention.Application.Users.Contracts;
using Vention.Application.Users.Queries.GetAllOnlineUsers;
using Vention.Application.Users.Queries.GetOnlineUsersByOrganization;

namespace Vention.API.Hubs
{

    public sealed class NotificationHub : Hub<INotificationClient>
    {
        public const string Route = "/hubs/notifications";

        private readonly IPresenceTracker _presenceTracker;
        private readonly IDispatcher _dispatcher;

        public NotificationHub(IPresenceTracker presenceTracker, IDispatcher dispatcher)
        {
            _presenceTracker = presenceTracker;
            _dispatcher = dispatcher;
        }

        public async Task JoinGroup(string groupName)
        {
            if (!PresenceGroups.TryParseOrganizationId(groupName, out _))
                throw new HubException("Only organization groups are supported. Group name must be 'org-{organizationId}'.");

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Context.Items["GroupName"] = groupName;

            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                return;

            var isNewlyOnline = await _presenceTracker.UserConnectedAsync(
                groupName,
                userId,
                Context.ConnectionId);

            if (isNewlyOnline)
                await Clients.Group(groupName).UserConnected(userId);

            var onlineUsers = await _presenceTracker.GetOnlineUsersAsync(groupName);
            await Clients.Caller.OnlineUsersSnapshot(onlineUsers);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Context.Items.Remove("GroupName");

            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                return;

            var isNowOffline = await _presenceTracker.UserDisconnectedAsync(
                groupName,
                userId,
                Context.ConnectionId);

            if (isNowOffline)
                await Clients.Group(groupName).UserDisconnected(userId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId) &&
                Context.Items.TryGetValue("GroupName", out var groupObj) &&
                groupObj is string groupName)
            {
                var isNowOffline = await _presenceTracker.UserDisconnectedAsync(
                    groupName,
                    userId,
                    Context.ConnectionId);

                if (isNowOffline)
                    await Clients.Group(groupName).UserDisconnected(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task<IReadOnlyList<OnlineUserResponse>> GetOnlineUsers(string groupName)
        {
            if (!PresenceGroups.TryParseOrganizationId(groupName, out var organizationId))
                throw new HubException("Group name must be in format 'org-{organizationId}'.");

            return await _dispatcher.Send(
                new GetOnlineUsersByOrganizationQuery(organizationId, GetRequiredActingUserId()));
        }

        public async Task<IReadOnlyList<OnlineUserResponse>> GetAllOnlineUsers()
        {
            return await _dispatcher.Send(
                new GetAllOnlineUsersQuery(GetRequiredActingUserId()));
        }

        private Guid GetRequiredActingUserId()
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var userId))
                throw new HubException("User is not authenticated.");

            return userId;
        }
    }
}