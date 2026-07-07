using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Commands.CreateChatSession
{
    public sealed record CreateChatSessionCommand(
        string Title,
        Guid OrganizationId,
        Guid CreatedByUserId,
        Guid ParticipantUserId) : ICommand<ChatSessionResponse>;
}
