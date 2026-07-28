using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Queries.GetChatSessionById
{
    public sealed record GetChatSessionByIdQuery(Guid Id, Guid RequestingUserId) : IQuery<ChatSessionResponse>;

}
