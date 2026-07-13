using Vention.Application.Messages.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Messages.Queries.GetChatMessageById
{
    public sealed record GetChatMessageByIdQuery(Guid Id) : IQuery<ChatMessageResponse>;
}
