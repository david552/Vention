using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Chats.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Chats.Commands.CreateChatSession
{
    public sealed class CreateChatSessionCommandHandler : ICommandHandler<CreateChatSessionCommand, ChatSessionResponse>
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateChatSessionCommandHandler(
            IChatSessionRepository chatSessionRepository,
            IChatSessionMemberRepository memberRepository,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork)
        {
            _chatSessionRepository = chatSessionRepository;
            _memberRepository = memberRepository;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatSessionResponse> Handle(CreateChatSessionCommand command, CancellationToken ct)
        {
            var organizationId = new OrganizationId(command.OrganizationId);
            var createdByUserId = new UserId(command.CreatedByUserId);
            var participantUserId = new UserId(command.ParticipantUserId);

            if (!await _organizationRepository.ExistsByIdAsync(organizationId, ct))
                throw new NotFoundException($"Organization '{command.OrganizationId}' was not found.");

            if (!await _userRepository.ExistsByIdAsync(createdByUserId, ct))
                throw new NotFoundException($"User '{command.CreatedByUserId}' was not found.");

            if (!await _userRepository.ExistsByIdAsync(participantUserId, ct))
                throw new NotFoundException($"User '{command.ParticipantUserId}' was not found.");

            if (!await _membershipRepository.ExistsAsync(createdByUserId, organizationId, ct))
                throw new InvalidOperationException($"User '{command.CreatedByUserId}' is not a member of organization '{command.OrganizationId}'.");

            if (!await _membershipRepository.ExistsAsync(participantUserId, organizationId, ct))
                throw new InvalidOperationException($"User '{command.ParticipantUserId}' is not a member of organization '{command.OrganizationId}'.");

            var chatSession = ChatSession.Create(command.Title, organizationId, createdByUserId);
            _chatSessionRepository.Add(chatSession);

            _memberRepository.Add(ChatSessionMember.Create(chatSession.Id, createdByUserId));
            _memberRepository.Add(ChatSessionMember.Create(chatSession.Id, participantUserId));

            await _unitOfWork.SaveChangesAsync(ct);

            return chatSession.Adapt<ChatSessionResponse>();
        }
    }
}
