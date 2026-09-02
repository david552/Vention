using Vention.Application.Abstractions;
using Vention.Application.Chats.Contracts;
using Vention.Application.Chats.Services;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;
using System.Data;

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
        private readonly INotificationPublisher _notificationPublisher;
        private readonly ChatSessionResponseMapper _mapper;

        public GetOrCreateDirectChatSessionCommandHandler(
            IChatSessionRepository sessionRepository,
            IChatSessionMemberRepository memberRepository,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork,
            INotificationPublisher notificationPublisher,
            ChatSessionResponseMapper mapper)
        {
            _sessionRepository = sessionRepository;
            _memberRepository = memberRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _membershipRepository = membershipRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationPublisher = notificationPublisher;
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

            if (await _userRepository.GetByIdAsync(participantId, ct) is null)
                throw new NotFoundException($"User '{command.ParticipantUserId}' was not found.");

            if (!await _userRepository.ExistsByIdAsync(initiatorId, ct))
                throw new NotFoundException($"User '{command.InitiatorUserId}' was not found.");

            if (!await _membershipRepository.ExistsAsync(initiatorId, organizationId, ct))
                throw new InvalidOperationException($"User '{command.InitiatorUserId}' is not a member of organization '{command.OrganizationId}'.");

            if (!await _membershipRepository.ExistsAsync(participantId, organizationId, ct))
                throw new InvalidOperationException($"User '{command.ParticipantUserId}' is not a member of organization '{command.OrganizationId}'.");

            var existing = await _memberRepository.FindDirectSessionAsync(initiatorId, participantId, organizationId, ct);
            if (existing is not null)
                return await _mapper.MapAsync(existing, initiatorId, ct);

            var session = ChatSession.CreateDirectChat(organizationId, initiatorId, participantId);

            _sessionRepository.Add(session);
            _memberRepository.Add(ChatSessionMember.Create(session.Id, initiatorId));
            _memberRepository.Add(ChatSessionMember.Create(session.Id, participantId));

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);

                var responseForInitiator = await _mapper.MapAsync(session, initiatorId, ct);

                var responseForParticipant = await _mapper.MapAsync(session, participantId, ct);

                await _notificationPublisher.NotifyChatSessionCreatedAsync(
                    participantId.Value,
                    responseForParticipant,
                    ct);

                return responseForInitiator;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                var raced = await _memberRepository.FindDirectSessionAsync(
                    initiatorId, participantId, organizationId, ct)
                    ?? throw new InvalidOperationException(
                        "Failed to create or find direct chat session after a concurrency conflict.");

                return await _mapper.MapAsync(raced, initiatorId, ct);
            }
        }
    }
}