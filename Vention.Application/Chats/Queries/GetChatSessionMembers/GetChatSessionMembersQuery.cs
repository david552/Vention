using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Chats.Queries.GetChatSessionMembers
{
    public sealed record GetChatSessionMembersQuery(Guid SessionId) : IQuery<IReadOnlyList<ChatSessionMemberResponse>>;
}
