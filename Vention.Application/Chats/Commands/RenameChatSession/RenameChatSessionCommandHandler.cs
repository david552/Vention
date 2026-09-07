using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Chats.Contracts;
using Vention.Application.Chats.Services;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Users;

namespace Vention.Application.Chats.Commands.RenameChatSession
{


    public sealed class RenameChatSessionCommandHandler : ICommandHandler<RenameChatSessionCommand, ChatSessionResponse>
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ChatAuthorizationService _chatAuth;
        private readonly ChatSessionResponseMapper _mapper;
        private readonly INotificationPublisher _notificationPublisher;

        public RenameChatSessionCommandHandler(
            IChatSessionRepository chatSessionRepository,
            IUnitOfWork unitOfWork,
            ChatAuthorizationService chatAuth,
            ChatSessionResponseMapper mapper,
            INotificationPublisher notificationPublisher)
        {
            _chatSessionRepository = chatSessionRepository;
            _unitOfWork = unitOfWork;
            _chatAuth = chatAuth;
            _mapper = mapper;
            _notificationPublisher = notificationPublisher;
        }
        public async Task<ChatSessionResponse> Handle(RenameChatSessionCommand command, CancellationToken ct)
        {
            await _chatAuth.EnsureIsSessionMemberAsync(command.Id, command.RequestingUserId, ct);

            var chatSession = await _chatSessionRepository.GetByIdAsync(new ChatSessionId(command.Id), ct)
                ?? throw new NotFoundException($"Chat session '{command.Id}' was not found.");

            chatSession.Rename(command.Title);
            await _unitOfWork.SaveChangesAsync(ct);

            await _notificationPublisher.NotifyChatRenamedAsync(
                chatSession.OrganizationId.Value,
                chatSession.Id.Value,
                chatSession.Title,
                ct);

            return await _mapper.MapAsync(chatSession, new UserId(command.RequestingUserId), ct);
        }
    }
}
