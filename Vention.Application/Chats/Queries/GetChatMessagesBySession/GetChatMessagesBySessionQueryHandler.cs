using Mapster;
using Vention.Application.Chats.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;

namespace Vention.Application.Chats.Queries.GetChatMessagesBySession
{
    public sealed class GetChatMessagesBySessionQueryHandler
        : IQueryHandler<GetChatMessagesBySessionQuery, IReadOnlyList<ChatMessageResponse>>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IChatSessionRepository _chatSessionRepository;

        public GetChatMessagesBySessionQueryHandler(
            IChatMessageRepository chatMessageRepository,
            IChatSessionRepository chatSessionRepository)
        {
            _chatMessageRepository = chatMessageRepository;
            _chatSessionRepository = chatSessionRepository;
        }

        public async Task<IReadOnlyList<ChatMessageResponse>> Handle(GetChatMessagesBySessionQuery query, CancellationToken ct)
        {
            var sessionId = new ChatSessionId(query.SessionId);

            if (!await _chatSessionRepository.ExistsByIdAsync(sessionId, ct))
                throw new NotFoundException($"Chat session '{query.SessionId}' was not found.");

            var messages = await _chatMessageRepository.GetBySessionIdAsync(sessionId, ct);
            return messages.Adapt<IReadOnlyList<ChatMessageResponse>>();
        }
    }
}
