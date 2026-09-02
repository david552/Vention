using Microsoft.AspNetCore.SignalR;

namespace Vention.API.Providers
{
    public class HeaderUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var httpContext = connection.GetHttpContext();

            var userId = httpContext?.Request.Headers["X-User-Id"].FirstOrDefault();

            return userId;
        }
    }
}