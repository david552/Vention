using Mapster;
using Vention.Application.Authorization;
using Vention.Application.Chats.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;

namespace Vention.Application.Chats.Queries.GetChatSessionById
{
    public sealed class GetChatSessionByIdQueryHandler : IQueryHandler<GetChatSessionByIdQuery, ChatSessionResponse>
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly ChatAuthorizationService _chatAuth;

        public GetChatSessionByIdQueryHandler(
            IChatSessionRepository chatSessionRepository,
            ChatAuthorizationService chatAuth)
        {
            _chatSessionRepository = chatSessionRepository;
            _chatAuth = chatAuth;
        } 

        public async Task<ChatSessionResponse> Handle(GetChatSessionByIdQuery query, CancellationToken ct)
        {
            await _chatAuth.EnsureIsSessionMemberAsync(query.Id, query.RequestingUserId, ct);

            var chatSession = await _chatSessionRepository.GetByIdAsync(new ChatSessionId(query.Id), ct)
                ?? throw new NotFoundException($"Chat session '{query.Id}' was not found.");

            return chatSession.Adapt<ChatSessionResponse>();
        }
    }
}
