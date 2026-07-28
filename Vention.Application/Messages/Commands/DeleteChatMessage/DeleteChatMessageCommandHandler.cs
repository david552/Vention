using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Messages;

namespace Vention.Application.Messages.Commands.DeleteChatMessage
{
    public sealed class DeleteChatMessageCommandHandler : ICommandHandler<DeleteChatMessageCommand>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ChatAuthorizationService _chatAuth;

        public DeleteChatMessageCommandHandler(
            IChatMessageRepository chatMessageRepository,
            IUnitOfWork unitOfWork,
            ChatAuthorizationService chatAuth)
        {
            _chatMessageRepository = chatMessageRepository;
            _unitOfWork = unitOfWork;
            _chatAuth = chatAuth;


        }

        public async Task Handle(DeleteChatMessageCommand command, CancellationToken ct)
        {
            var message = await _chatMessageRepository.GetByIdAsync(new ChatMessageId(command.Id), ct)
                ?? throw new NotFoundException($"Chat message '{command.Id}' was not found.");

            await _chatAuth.EnsureIsSessionMemberAsync(
               message.ChatSessionId.Value,
               command.RequestingUserId,
               ct);

            if (message.SenderId.Value != command.RequestingUserId)
                throw new ForbiddenException("You can only delete your own messages.");

            _chatMessageRepository.Remove(message);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
