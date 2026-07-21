using Mapster;
using Vention.Application.Common;
using Vention.Application.Exceptions;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Messages;
using Vention.Domain.Users;
namespace Vention.Application.Messages.Queries.GetChatMessagesBySession
{
    public sealed class GetChatMessagesBySessionQueryHandler
        : IQueryHandler<GetChatMessagesBySessionQuery, CursorPage<ChatMessageResponse>>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IChatSessionMemberRepository _memberRepository;

        public GetChatMessagesBySessionQueryHandler(
            IChatMessageRepository chatMessageRepository,
            IChatSessionRepository chatSessionRepository,
            IChatSessionMemberRepository memberRepository)
        {
            _chatMessageRepository = chatMessageRepository;
            _chatSessionRepository = chatSessionRepository;
            _memberRepository = memberRepository;
        }
        public async Task<CursorPage<ChatMessageResponse>> Handle(
          GetChatMessagesBySessionQuery query, CancellationToken ct)
        {
            var sessionId = new ChatSessionId(query.SessionId);
            var requestingUserId = new UserId(query.RequestingUserId);


            if (!await _chatSessionRepository.ExistsByIdAsync(sessionId, ct))
                throw new NotFoundException($"Chat session '{query.SessionId}' was not found.");

            if (!await _memberRepository.IsMemberAsync(sessionId, requestingUserId, ct))
                throw new ForbiddenException(
                    $"User '{query.RequestingUserId}' is not a participant of chat session '{query.SessionId}'.");

            var pageSize = CursorCodec.NormalizePageSize(query.PageSize);
            DateTimeOffset? cursorCreatedAt = null;
            long? cursorSequence = null;

            if (query.Cursor is not null)
            {
                var decoded = CursorCodec.Decode(query.Cursor);
                cursorCreatedAt = decoded.SortValue;
                cursorSequence = decoded.Sequence;
            }

            var rows = await _chatMessageRepository.GetPageBySessionIdAsync(
                sessionId, cursorCreatedAt, cursorSequence, pageSize + 1, ct);

            string? nextCursor = null;
            if (rows.Count > pageSize)
            {
                var last = rows[pageSize - 1];
                nextCursor = CursorCodec.Encode(last.Message.CreatedAt, last.Sequence);
                rows = rows.Take(pageSize).ToList();
            }
            var messages = rows.Select(r => r.Message).ToList();
            return new CursorPage<ChatMessageResponse>(
                messages.Adapt<IReadOnlyList<ChatMessageResponse>>(),
                nextCursor);
        }
    }
}