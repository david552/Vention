using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;

namespace Vention.Application.Chats.Commands.DeleteChatMessage
{
    public sealed class DeleteChatMessageCommandHandler : ICommandHandler<DeleteChatMessageCommand>
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteChatMessageCommandHandler(IChatMessageRepository chatMessageRepository, IUnitOfWork unitOfWork)
        {
            _chatMessageRepository = chatMessageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteChatMessageCommand command, CancellationToken ct)
        {
            var message = await _chatMessageRepository.GetByIdAsync(new ChatMessageId(command.Id), ct)
                ?? throw new NotFoundException($"Chat message '{command.Id}' was not found.");

            _chatMessageRepository.Remove(message);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
