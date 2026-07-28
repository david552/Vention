using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;

namespace Vention.Application.Users.Queries.GetUserById
{
    public sealed record GetUserByIdQuery(Guid Id, Guid ActingUserId) : IQuery<UserResponse>;

}
