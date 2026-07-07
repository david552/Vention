using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Queries.GetSessionsForUser
{
    public sealed record GetSessionsForUserQuery(Guid UserId, Guid OrganizationId) : IQuery<IReadOnlyList<ChatSessionResponse>>;
}
