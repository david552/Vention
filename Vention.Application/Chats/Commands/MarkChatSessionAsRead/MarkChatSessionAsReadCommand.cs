using Vention.Application.Messaging;

namespace Vention.Application.Chats.Commands.MarkChatSessionAsRead
{

    public sealed record MarkChatSessionAsReadCommand(
        Guid SessionId,
        Guid UserId) : ICommand;
}