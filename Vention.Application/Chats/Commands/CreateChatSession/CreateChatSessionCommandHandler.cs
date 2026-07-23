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
            var initiatorId = new UserId(command.InitiatorUserId);
            var participantId = new UserId(command.ParticipantUserId);

            if (!await _organizationRepository.ExistsByIdAsync(organizationId, ct))
                throw new NotFoundException($"Organization '{command.OrganizationId}' was not found.");

            if (!await _userRepository.ExistsByIdAsync(initiatorId, ct))
                throw new NotFoundException($"User '{command.InitiatorUserId}' was not found.");

            if (!await _userRepository.ExistsByIdAsync(participantId, ct))
                throw new NotFoundException($"User '{command.ParticipantUserId}' was not found.");

            if (!await _membershipRepository.ExistsAsync(initiatorId, organizationId, ct))
                throw new InvalidOperationException($"User '{command.InitiatorUserId}' is not a member of organization '{command.OrganizationId}'.");

            if (!await _membershipRepository.ExistsAsync(participantId, organizationId, ct))
                throw new InvalidOperationException($"User '{command.ParticipantUserId}' is not a member of organization '{command.OrganizationId}'.");

            var chatSession = ChatSession.CreateDirectChat(organizationId, initiatorId, participantId);
            _chatSessionRepository.Add(chatSession);

            _memberRepository.Add(ChatSessionMember.Create(chatSession.Id, initiatorId));
            _memberRepository.Add(ChatSessionMember.Create(chatSession.Id, participantId));

            await _unitOfWork.SaveChangesAsync(ct);

            return chatSession.Adapt<ChatSessionResponse>();
        }
    }
}
