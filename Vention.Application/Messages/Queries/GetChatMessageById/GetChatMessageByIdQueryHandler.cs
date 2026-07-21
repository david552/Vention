using Mapster;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Messages;

namespace Vention.Application.Messages.Queries.GetChatMessageById
{
    public sealed class GetChatMessageByIdQueryHandler : IQueryHandler<GetChatMessageByIdQuery, ChatMessageResponse>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly ChatAuthorizationService _chatAuth;

        public GetChatMessageByIdQueryHandler(
            IChatMessageRepository chatMessageRepository,
            ChatAuthorizationService chatAuth)
        {
            _chatMessageRepository = chatMessageRepository;
            _chatAuth = chatAuth;
        }

        public async Task<ChatMessageResponse> Handle(GetChatMessageByIdQuery query, CancellationToken ct)
        {
            var message = await _chatMessageRepository.GetByIdAsync(new ChatMessageId(query.Id), ct)
                ?? throw new NotFoundException($"Chat message '{query.Id}' was not found.");

            await _chatAuth.EnsureIsSessionMemberAsync(
                message.ChatSessionId.Value,
                query.RequestingUserId,
                ct);

            return message.Adapt<ChatMessageResponse>();
        }
    }
}
