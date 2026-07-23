using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Commands.CreateChatSession
{
    public sealed record CreateChatSessionCommand(
        Guid OrganizationId,
        Guid InitiatorUserId,
        Guid ParticipantUserId) : ICommand<ChatSessionResponse>;
}
