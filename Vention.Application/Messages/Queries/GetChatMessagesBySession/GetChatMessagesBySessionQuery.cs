using Vention.Application.Common;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Messages.Queries.GetChatMessagesBySession
{
    public sealed record GetChatMessagesBySessionQuery(
        Guid SessionId,
        Guid RequestingUserId,
        bool Paginated = false,
        string? Cursor = null,
        int PageSize = 50) : IQuery<ListResult<ChatMessageResponse>>;
}
