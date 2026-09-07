using Vention.Application.Chats.Contracts;
using Vention.Application.Chats.Services;
using Vention.Application.Common;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Chats.Queries.GetSessionsForUser
{

    public sealed class GetSessionsForUserQueryHandler
        : IQueryHandler<GetChatSessionsForUserQuery, ListResult<ChatSessionResponse>>
    {
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ChatSessionResponseMapper _mapper;

        public GetSessionsForUserQueryHandler(
            IChatSessionMemberRepository memberRepository,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            ChatSessionResponseMapper mapper)
        {
            _memberRepository = memberRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _mapper = mapper;
        }

        public async Task<ListResult<ChatSessionResponse>> Handle(
            GetChatSessionsForUserQuery query,
            CancellationToken ct)
        {
            var userId = new UserId(query.UserId);
            var organizationId = new OrganizationId(query.OrganizationId);

            if (!await _userRepository.ExistsByIdAsync(userId, ct))
                throw new NotFoundException($"User '{query.UserId}' was not found.");

            if (!await _organizationRepository.ExistsByIdAsync(organizationId, ct))
                throw new NotFoundException($"Organization '{query.OrganizationId}' was not found.");

            if (!query.Paginated)
            {
                var allSessions = await _memberRepository.GetSessionsForUserAsync(userId, organizationId, ct);
                var allItems = await _mapper.MapManyAsync(allSessions, userId, ct);

                return new ListResult<ChatSessionResponse>(allItems, NextCursor: null, Paginated: false);
            }

            var pageSize = CursorCodec.NormalizePageSize(query.PageSize);
            DateTimeOffset? cursorUpdatedAt = null;
            long? cursorSequence = null;

            if (query.Cursor is not null)
            {
                var decoded = CursorCodec.Decode(query.Cursor);
                cursorUpdatedAt = decoded.SortValue;
                cursorSequence = decoded.Sequence;
            }

            var rows = await _memberRepository.GetSessionsForUserPageAsync(
                userId,
                organizationId,
                cursorUpdatedAt,
                cursorSequence,
                pageSize + 1,
                ct);

            string? nextCursor = null;

            if (rows.Count > pageSize)
            {
                var last = rows[pageSize - 1];
                nextCursor = CursorCodec.Encode(last.Session.UpdatedAt, last.Sequence);
                rows = rows.Take(pageSize).ToList();
            }

            var sessions = rows.Select(r => r.Session).ToList();
            var items = await _mapper.MapManyAsync(sessions, userId, ct);

            return new ListResult<ChatSessionResponse>(items, nextCursor, Paginated: true);
        }
    }
}