namespace Vention.Application.Abstractions
{
    public interface IPresenceTracker
    {
        Task<bool> UserConnectedAsync(string groupName, string userId, string connectionId);
        Task<bool> UserDisconnectedAsync(string groupName, string userId, string connectionId);
        Task<IReadOnlyList<string>> GetOnlineUsersAsync(string groupName);
        Task<IReadOnlyList<string>> GetAllOnlineUsersAsync();
    }
}