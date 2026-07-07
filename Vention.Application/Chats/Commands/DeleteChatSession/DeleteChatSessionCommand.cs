using Vention.Application.Messaging;


namespace Vention.Application.Chats.Commands.DeleteChatSession
{
    public sealed record DeleteChatSessionCommand(Guid Id) : ICommand;

}
