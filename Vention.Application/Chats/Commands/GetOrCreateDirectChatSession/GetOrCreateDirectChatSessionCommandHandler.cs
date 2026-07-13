using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Chats.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Chats.Commands.GetOrCreateDirectChatSession
{
    public sealed class GetOrCreateDirectChatSessionCommandHandler
        : ICommandHandler<GetOrCreateDirectChatSessionCommand, ChatSessionResponse>
    {
        private readonly IChatSessionRepository _sessionRepository;
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetOrCreateDirectChatSessionCommandHandler(
            IChatSessionRepository sessionRepository,
            IChatSessionMemberRepository memberRepository,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork)
        {
            _sessionRepository = sessionRepository;
            _memberRepository = memberRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatSessionResponse> Handle(GetOrCreateDirectChatSessionCommand command, CancellationToken ct)
        {
            var organizationId = new OrganizationId(command.OrganizationId);
            var initiatorId = new UserId(command.InitiatorUserId);
            var participantId = new UserId(command.ParticipantUserId);

            if (command.InitiatorUserId == command.ParticipantUserId)
                throw new InvalidOperationException("Cannot open a direct chat session with yourself.");

            if (!await _organizationRepository.ExistsByIdAsync(organizationId, ct))
                throw new NotFoundException($"Organization '{command.OrganizationId}' was not found.");

            var participant = await _userRepository.GetByIdAsync(participantId, ct)
                ?? throw new NotFoundException($"User '{command.ParticipantUserId}' was not found.");

            if (!await _userRepository.ExistsByIdAsync(initiatorId, ct))
                throw new NotFoundException($"User '{command.InitiatorUserId}' was not found.");

            if (!await _membershipRepository.ExistsAsync(initiatorId, organizationId, ct))
                throw new InvalidOperationException($"User '{command.InitiatorUserId}' is not a member of organization '{command.OrganizationId}'.");

            if (!await _membershipRepository.ExistsAsync(participantId, organizationId, ct))
                throw new InvalidOperationException($"User '{command.ParticipantUserId}' is not a member of organization '{command.OrganizationId}'.");

            var existing = await _memberRepository.FindDirectSessionAsync(initiatorId, participantId, organizationId, ct);
            if (existing is not null)
                return existing.Adapt<ChatSessionResponse>();

            var session = ChatSession.Create(participant.Name, organizationId, initiatorId);
            _sessionRepository.Add(session);

            _memberRepository.Add(ChatSessionMember.Create(session.Id, initiatorId));
            _memberRepository.Add(ChatSessionMember.Create(session.Id, participantId));

            await _unitOfWork.SaveChangesAsync(ct);

            return session.Adapt<ChatSessionResponse>();
        }
    }
}
