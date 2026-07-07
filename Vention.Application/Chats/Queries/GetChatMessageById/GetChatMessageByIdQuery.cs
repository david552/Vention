using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Queries.GetChatMessageById
{
    public sealed record GetChatMessageByIdQuery(Guid Id) : IQuery<ChatMessageResponse>;
}
