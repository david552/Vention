using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Messages;
using Vention.Domain.Users;

namespace Vention.Application.Messages.Commands.SendChatMessage
{
    public sealed class SendChatMessageCommandHandler : ICommandHandler<SendChatMessageCommand, ChatMessageResponse>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SendChatMessageCommandHandler(
            IChatMessageRepository chatMessageRepository,
            IChatSessionMemberRepository memberRepository,
            IChatSessionRepository chatSessionRepository,
            IUnitOfWork unitOfWork)
        {
            _chatMessageRepository = chatMessageRepository;
            _memberRepository = memberRepository;
            _chatSessionRepository = chatSessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatMessageResponse> Handle(SendChatMessageCommand command, CancellationToken ct)
        {
            var sessionId = new ChatSessionId(command.ChatSessionId);
            var senderId = new UserId(command.SenderId);

            var session = await _chatSessionRepository.GetByIdAsync(sessionId, ct)
               ?? throw new NotFoundException($"Chat session '{command.ChatSessionId}' was not found.");

            if (!await _memberRepository.IsMemberAsync(sessionId, senderId, ct))
                throw new InvalidOperationException(
                    $"User '{command.SenderId}' is not a participant of chat session '{command.ChatSessionId}'.");

            var message = ChatMessage.Create(sessionId, senderId, command.Content);

            session.Touch();
            _chatMessageRepository.Add(message);
            await _unitOfWork.SaveChangesAsync(ct);

            return message.Adapt<ChatMessageResponse>();
        }
    }
}
