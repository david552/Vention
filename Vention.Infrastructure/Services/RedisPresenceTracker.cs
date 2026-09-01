using StackExchange.Redis;
using Vention.Application.Abstractions;

namespace Vention.Infrastructure.Services
{

    public sealed class RedisPresenceTracker : IPresenceTracker
    {
        private readonly IConnectionMultiplexer _redis;

        private const string GlobalOnlineUsersKey = "presence:global:online_users";
        private static string GlobalUserConnectionsKey(string userId) =>
            $"presence:global:user:{userId}:connections";

        private static string GroupOnlineUsersKey(string groupName) =>
            $"presence:{groupName}:online_users";

        private static string GroupUserConnectionsKey(string groupName, string userId) =>
            $"presence:{groupName}:user:{userId}:connections";

        public RedisPresenceTracker(IConnectionMultiplexer redis) => _redis = redis;

        public async Task<bool> UserConnectedAsync(string groupName, string userId, string connectionId)
        {
            var db = _redis.GetDatabase();
            var groupUserKey = GroupUserConnectionsKey(groupName, userId);

            await db.SetAddAsync(groupUserKey, connectionId);
            await db.KeyExpireAsync(groupUserKey, TimeSpan.FromHours(24));
            await db.SetAddAsync(GroupOnlineUsersKey(groupName), userId);

            var globalUserKey = GlobalUserConnectionsKey(userId);

            await db.SetAddAsync(globalUserKey, connectionId);
            await db.KeyExpireAsync(globalUserKey, TimeSpan.FromHours(24));
            await db.SetAddAsync(GlobalOnlineUsersKey, userId);

            return await db.SetLengthAsync(groupUserKey) == 1;
        }

        public async Task<bool> UserDisconnectedAsync(string groupName, string userId, string connectionId)
        {
            var db = _redis.GetDatabase();
            var groupUserKey = GroupUserConnectionsKey(groupName, userId);
            await db.SetRemoveAsync(groupUserKey, connectionId);

            var isOfflineInGroup = false;

            if (await db.SetLengthAsync(groupUserKey) == 0)
            {
                await db.KeyDeleteAsync(groupUserKey);
                await db.SetRemoveAsync(GroupOnlineUsersKey(groupName), userId);
                isOfflineInGroup = true;
            }

            var globalUserKey = GlobalUserConnectionsKey(userId);
            await db.SetRemoveAsync(globalUserKey, connectionId);

            if (await db.SetLengthAsync(globalUserKey) == 0)
            {
                await db.KeyDeleteAsync(globalUserKey);
                await db.SetRemoveAsync(GlobalOnlineUsersKey, userId);
            }

            return isOfflineInGroup;
        }

        public async Task<IReadOnlyList<string>> GetOnlineUsersAsync(string groupName)
        {
            var db = _redis.GetDatabase();
            var members = await db.SetMembersAsync(GroupOnlineUsersKey(groupName));
            var online = new List<string>();

            foreach (var member in members)
            {
                var userId = member.ToString();
                var connectionKey = GroupUserConnectionsKey(groupName, userId);
                var hasActiveConnections = await db.SetLengthAsync(connectionKey) > 0;

                if (hasActiveConnections)
                {
                    online.Add(userId);
                }
                else
                {
                    await db.SetRemoveAsync(GroupOnlineUsersKey(groupName), userId);
                }
            }

            return online;
        }

        public async Task<IReadOnlyList<string>> GetAllOnlineUsersAsync()
        {
            var db = _redis.GetDatabase();
            var members = await db.SetMembersAsync(GlobalOnlineUsersKey);
            var online = new List<string>();

            foreach (var member in members)
            {
                var userId = member.ToString();
                var connectionKey = GlobalUserConnectionsKey(userId);
                var hasActiveConnections = await db.SetLengthAsync(connectionKey) > 0;

                if (hasActiveConnections)
                {
                    online.Add(userId);
                }
                else
                {
                    await db.SetRemoveAsync(GlobalOnlineUsersKey, userId);
                }
            }

            return online;
        }
    }
}