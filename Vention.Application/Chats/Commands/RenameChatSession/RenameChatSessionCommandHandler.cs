using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Chats.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;

namespace Vention.Application.Chats.Commands.RenameChatSession
{
    public sealed class RenameChatSessionCommandHandler : ICommandHandler<RenameChatSessionCommand, ChatSessionResponse>
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ChatAuthorizationService _chatAuth;


        public RenameChatSessionCommandHandler(
            IChatSessionRepository chatSessionRepository,
            IUnitOfWork unitOfWork,
            ChatAuthorizationService chatAuth)
        {
            _chatSessionRepository = chatSessionRepository;
            _unitOfWork = unitOfWork;
            _chatAuth = chatAuth;

        }

        public async Task<ChatSessionResponse> Handle(RenameChatSessionCommand command, CancellationToken ct)
        {
            await _chatAuth.EnsureIsSessionMemberAsync(command.Id, command.RequestingUserId, ct);

            var chatSession = await _chatSessionRepository.GetByIdAsync(new ChatSessionId(command.Id), ct)
                ?? throw new NotFoundException($"Chat session '{command.Id}' was not found.");

            chatSession.Rename(command.Title);
            await _unitOfWork.SaveChangesAsync(ct);

            return chatSession.Adapt<ChatSessionResponse>();
        }
    }
}
