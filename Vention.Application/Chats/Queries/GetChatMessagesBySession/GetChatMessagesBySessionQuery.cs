using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Queries.GetChatMessagesBySession
{
    public sealed record GetChatMessagesBySessionQuery(Guid SessionId) : IQuery<IReadOnlyList<ChatMessageResponse>>;
}
