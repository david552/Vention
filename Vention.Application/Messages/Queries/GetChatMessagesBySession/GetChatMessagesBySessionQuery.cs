using Vention.Application.Common;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Messages.Queries.GetChatMessagesBySession
{
    public sealed record GetChatMessagesBySessionQuery(
        Guid SessionId,
        Guid RequestingUserId,
        string? Cursor,
        int PageSize = 50) : IQuery<CursorPage<ChatMessageResponse>>;
}
