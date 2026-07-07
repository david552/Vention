using Vention.Application.Messaging;

namespace Vention.Application.Users.Commands.DeleteUser
{
    public sealed record DeleteUserCommand(Guid Id) : ICommand;

}
