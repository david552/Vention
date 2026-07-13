using Mapster;
using Vention.Application.Chats.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;

namespace Vention.Application.Chats.Queries.GetChatSessionMembers
{
    public sealed class GetChatSessionMembersQueryHandler
        : IQueryHandler<GetChatSessionMembersQuery, IReadOnlyList<ChatSessionMemberResponse>>
    {
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IChatSessionRepository _sessionRepository;

        public GetChatSessionMembersQueryHandler(
            IChatSessionMemberRepository memberRepository,
            IChatSessionRepository sessionRepository)
        {
            _memberRepository = memberRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<IReadOnlyList<ChatSessionMemberResponse>> Handle(GetChatSessionMembersQuery query, CancellationToken ct)
        {
            var sessionId = new ChatSessionId(query.SessionId);

            if (!await _sessionRepository.ExistsByIdAsync(sessionId, ct))
                throw new NotFoundException($"Chat session '{query.SessionId}' was not found.");

            var members = await _memberRepository.GetBySessionIdAsync(sessionId, ct);
            return members.Adapt<IReadOnlyList<ChatSessionMemberResponse>>();
        }
    }
}
