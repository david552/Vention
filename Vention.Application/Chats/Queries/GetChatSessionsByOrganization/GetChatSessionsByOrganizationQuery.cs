using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Queries.GetChatSessionsByOrganization
{
    public sealed record GetChatSessionsByOrganizationQuery(Guid OrganizationId) : IQuery<IReadOnlyList<ChatSessionResponse>>;

}
