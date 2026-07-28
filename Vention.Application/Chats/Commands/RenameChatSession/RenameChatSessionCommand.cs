using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Commands.RenameChatSession
{
    public sealed record RenameChatSessionCommand(Guid Id, string Title, Guid RequestingUserId) : ICommand<ChatSessionResponse>;

}
