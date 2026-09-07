using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Messages;
using Vention.Domain.Users;

namespace Vention.Application.Chats.Commands.MarkChatSessionAsRead
{

    public sealed class MarkChatSessionAsReadCommandHandler
        : ICommandHandler<MarkChatSessionAsReadCommand>
    {
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkChatSessionAsReadCommandHandler(
            IChatSessionMemberRepository memberRepository,
            IChatMessageRepository messageRepository,
            IUnitOfWork unitOfWork)
        {
            _memberRepository = memberRepository;
            _messageRepository = messageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(MarkChatSessionAsReadCommand command, CancellationToken ct)
        {
            var sessionId = new ChatSessionId(command.SessionId);
            var userId = new UserId(command.UserId);

            var membership = await _memberRepository.GetMembershipAsync(sessionId, userId, ct)
                ?? throw new ForbiddenException(
                    $"User '{command.UserId}' is not a participant of chat session '{command.SessionId}'.");

            var latest = await _messageRepository.GetLatestBySessionIdAsync(sessionId, ct);
            var readAt = latest?.CreatedAt ?? DateTimeOffset.UtcNow;

            membership.MarkAsRead(readAt);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}