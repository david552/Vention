using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;

namespace Vention.Application.Users.Commands.CreateUser
{
    public sealed record CreateUserCommand(string Email, string Name, string Password) : ICommand<UserResponse>;

}
