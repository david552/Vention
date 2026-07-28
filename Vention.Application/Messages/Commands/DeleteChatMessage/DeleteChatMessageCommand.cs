using Vention.Application.Messaging;

namespace Vention.Application.Messages.Commands.DeleteChatMessage
{
    public sealed record DeleteChatMessageCommand(Guid Id, Guid RequestingUserId) : ICommand;
}
