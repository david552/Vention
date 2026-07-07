using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Commands.SendChatMessage
{
    public sealed record SendChatMessageCommand(
        Guid ChatSessionId,
        Guid SenderId,
        string Content) : ICommand<ChatMessageResponse>;
}
