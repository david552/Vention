using Mapster;
using Vention.Application.Chats.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;

namespace Vention.Application.Chats.Queries.GetChatMessageById
{
    public sealed class GetChatMessageByIdQueryHandler : IQueryHandler<GetChatMessageByIdQuery, ChatMessageResponse>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        public GetChatMessageByIdQueryHandler(IChatMessageRepository chatMessageRepository) => _chatMessageRepository = chatMessageRepository;

        public async Task<ChatMessageResponse> Handle(GetChatMessageByIdQuery query, CancellationToken ct)
        {
            var message = await _chatMessageRepository.GetByIdAsync(new ChatMessageId(query.Id), ct)
                ?? throw new NotFoundException($"Chat message '{query.Id}' was not found.");

            return message.Adapt<ChatMessageResponse>();
        }
    }
}
