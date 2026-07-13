using Vention.Application.Chats.Contracts;
using Vention.Application.Common;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Queries.GetSessionsForUser
{
    public sealed record GetSessionsForUserQuery(
          Guid UserId,
          Guid OrganizationId,
          string? Cursor,
          int PageSize = 50) : IQuery<CursorPage<ChatSessionResponse>>;
}
