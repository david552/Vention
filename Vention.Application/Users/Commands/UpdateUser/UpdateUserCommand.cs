using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;

namespace Vention.Application.Users.Commands.UpdateUser
{
    public sealed record UpdateUserCommand(Guid Id, string Name, Guid ActingUserId) : ICommand<UserResponse>;

}
