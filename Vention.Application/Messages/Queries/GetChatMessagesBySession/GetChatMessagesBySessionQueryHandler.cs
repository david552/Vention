using Vention.Application.Common;
using Vention.Application.Exceptions;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messages.Services;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Messages;
using Vention.Domain.Users;

namespace Vention.Application.Messages.Queries.GetChatMessagesBySession
{

    public sealed class GetChatMessagesBySessionQueryHandler
        : IQueryHandler<GetChatMessagesBySessionQuery, ListResult<ChatMessageResponse>>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IUserRepository _userRepository;



        public GetChatMessagesBySessionQueryHandler(
            IChatMessageRepository chatMessageRepository,
            IChatSessionRepository chatSessionRepository,
            IChatSessionMemberRepository memberRepository,
            IUserRepository userRepository)
        {
            _chatMessageRepository = chatMessageRepository;
            _chatSessionRepository = chatSessionRepository;
            _memberRepository = memberRepository;
            _userRepository = userRepository;
        }

        public async Task<ListResult<ChatMessageResponse>> Handle(
            GetChatMessagesBySessionQuery query,
            CancellationToken ct)
        {
            var sessionId = new ChatSessionId(query.SessionId);
            var requestingUserId = new UserId(query.RequestingUserId);

            if (!await _chatSessionRepository.ExistsByIdAsync(sessionId, ct))
                throw new NotFoundException($"Chat session '{query.SessionId}' was not found.");

            if (!await _memberRepository.IsMemberAsync(sessionId, requestingUserId, ct))
                throw new ForbiddenException(
                    $"User '{query.RequestingUserId}' is not a participant of chat session '{query.SessionId}'.");

            ListResult<ChatMessageResponse> result;

            if (!query.Paginated)
            {
                var allMessages = await _chatMessageRepository.GetBySessionIdAsync(sessionId, ct);
                var items = await MapMessagesAsync(allMessages, requestingUserId, ct);
                result = new ListResult<ChatMessageResponse>(items, NextCursor: null, Paginated: false);
            }
            else
            {
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
                    var oldestInPage = rows[0];
                    nextCursor = CursorCodec.Encode(oldestInPage.Message.CreatedAt, oldestInPage.Sequence);
                    rows = rows.TakeLast(pageSize).ToList();
                }

                var messages = rows.Select(r => r.Message).ToList();
                var pageItems = await MapMessagesAsync(messages, requestingUserId, ct);
                result = new ListResult<ChatMessageResponse>(pageItems, nextCursor, Paginated: true);
            }

            return result;
        }

        private async Task<IReadOnlyList<ChatMessageResponse>> MapMessagesAsync(
            IReadOnlyList<ChatMessage> messages,
            UserId requestingUserId,
            CancellationToken ct)
        {
            if (messages.Count == 0)
                return Array.Empty<ChatMessageResponse>();

            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
            var users = await _userRepository.GetByIdsAsync(senderIds, ct);
            var usersById = users.ToDictionary(u => u.Id);

            return ChatMessageResponseMapper.MapMany(messages, usersById, requestingUserId);
        }

       
    }
}