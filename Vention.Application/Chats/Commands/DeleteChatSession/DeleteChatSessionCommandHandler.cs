using Vention.Application.Abstractions;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;

namespace Vention.Application.Chats.Commands.DeleteChatSession
{
    public sealed class DeleteChatSessionCommandHandler : ICommandHandler<DeleteChatSessionCommand>
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteChatSessionCommandHandler(IChatSessionRepository chatSessionRepository, IUnitOfWork unitOfWork)
        {
            _chatSessionRepository = chatSessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteChatSessionCommand command, CancellationToken ct)
        {
            var chatSession = await _chatSessionRepository.GetByIdAsync(new ChatSessionId(command.Id), ct)
                ?? throw new NotFoundException($"Chat session '{command.Id}' was not found.");

            _chatSessionRepository.Remove(chatSession);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
