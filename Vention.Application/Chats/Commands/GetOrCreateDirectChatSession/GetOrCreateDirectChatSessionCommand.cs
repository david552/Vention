using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Commands.GetOrCreateDirectChatSession
{

    public sealed record GetOrCreateDirectChatSessionCommand(
        Guid OrganizationId,
        Guid InitiatorUserId,
        Guid ParticipantUserId) : ICommand<ChatSessionResponse>;
}
