using Mapster;
using System.Reflection;
using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messages.Contracts;
using Vention.Application.Messages.Services;
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
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationPublisher _notificationPublisher;


        public SendChatMessageCommandHandler(
            IChatMessageRepository chatMessageRepository,
            IChatSessionMemberRepository memberRepository,
            IChatSessionRepository chatSessionRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            INotificationPublisher notificationPublisher)
        {
            _chatMessageRepository = chatMessageRepository;
            _memberRepository = memberRepository;
            _chatSessionRepository = chatSessionRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _notificationPublisher = notificationPublisher;
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

            var sender = await _userRepository.GetByIdAsync(senderId, ct)
                ?? throw new NotFoundException($"User '{command.SenderId}' was not found.");

            var message = ChatMessage.Create(sessionId, senderId, command.Content);

            session.Touch();

            _chatMessageRepository.Add(message);

            await _unitOfWork.SaveChangesAsync(ct);

            var members = await _memberRepository.GetBySessionIdAsync(sessionId, ct);

            var recipient = members.FirstOrDefault(m => m.UserId != senderId)
                ?? throw new InvalidOperationException(
                    $"Chat session '{command.ChatSessionId}' has no recipient for sender '{command.SenderId}'.");

            await _notificationPublisher.NotifyUserMessageAsync(
                recipient.UserId.Value,
                session.Id.Value,
                message.Id.Value,
                message.SenderId.Value,  
                message.Content,
                message.CreatedAt,
                ct);

            var requestingUserId = senderId; 

            return ChatMessageResponseMapper.Map(message, sender, requestingUserId);
        }
    }
}
