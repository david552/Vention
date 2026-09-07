using Vention.Application.Chats.Contracts;
using Vention.Application.Common;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Queries.GetSessionsForUser
{
    public sealed record GetChatSessionsForUserQuery(
          Guid UserId,
          Guid OrganizationId,
          bool Paginated = false,
          string? Cursor = null,
          int PageSize = 50) : IQuery<ListResult<ChatSessionResponse>>;
}
