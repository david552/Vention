using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;

namespace Vention.Application.Users.Queries.GetUsers
{
    public sealed record GetUsersQuery : IQuery<IReadOnlyList<UserResponse>>;

}
