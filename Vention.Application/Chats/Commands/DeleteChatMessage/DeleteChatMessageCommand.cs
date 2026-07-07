using Vention.Application.Messaging;

namespace Vention.Application.Chats.Commands.DeleteChatMessage
{
    public sealed record DeleteChatMessageCommand(Guid Id) : ICommand;
}
