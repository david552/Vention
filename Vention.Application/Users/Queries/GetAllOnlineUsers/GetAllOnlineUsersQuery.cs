using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;

namespace Vention.Application.Users.Queries.GetAllOnlineUsers
{

    public sealed record GetAllOnlineUsersQuery(Guid ActingUserId)
        : IQuery<IReadOnlyList<OnlineUserResponse>>;
}