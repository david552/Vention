using Vention.Application.Messages.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Messages.Commands.SendChatMessage
{
    public sealed record SendChatMessageCommand(
        Guid ChatSessionId,
        Guid SenderId,
        string Content) : ICommand<ChatMessageResponse>;
}
