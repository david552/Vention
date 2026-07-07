using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Chats.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Users;

namespace Vention.Application.Chats.Commands.SendChatMessage
{
    public sealed class SendChatMessageCommandHandler : ICommandHandler<SendChatMessageCommand, ChatMessageResponse>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SendChatMessageCommandHandler(
            IChatMessageRepository chatMessageRepository,
            IChatSessionMemberRepository memberRepository,
            IUnitOfWork unitOfWork)
        {
            _chatMessageRepository = chatMessageRepository;
            _memberRepository = memberRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatMessageResponse> Handle(SendChatMessageCommand command, CancellationToken ct)
        {
            var sessionId = new ChatSessionId(command.ChatSessionId);
            var senderId = new UserId(command.SenderId);

            if (!await _memberRepository.IsMemberAsync(sessionId, senderId, ct))
                throw new InvalidOperationException(
                    $"User '{command.SenderId}' is not a participant of chat session '{command.ChatSessionId}'.");

            var message = ChatMessage.Create(sessionId, senderId, command.Content);

            _chatMessageRepository.Add(message);
            await _unitOfWork.SaveChangesAsync(ct);

            return message.Adapt<ChatMessageResponse>();
        }
    }
}
