using Mapster;
using Vention.Application.Abstractions;
using Vention.Application.Chats.Contracts;
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


            
            //  SAMPLE — explicit transaction + isolation level
            // ------------------------------------------------------------
            // For most handlers in this project stage this is NOT required:
            // - one SaveChangesAsync already runs in an implicit EF transaction
            // - unique indexes handle duplicate/concurrency for direct chats
            //
            // This block is kept as a deliberate sample of:
            // - Transaction management (Begin / SaveChanges / Commit / Rollback)
            // - Isolation levels (explicit ReadCommitted; can switch later)
            

            var session = ChatSession.CreateDirectChat(organizationId, initiatorId, participantId);

            _sessionRepository.Add(session);
            _memberRepository.Add(ChatSessionMember.Create(session.Id, initiatorId));
            _memberRepository.Add(ChatSessionMember.Create(session.Id, participantId));

            // 4.3: PostgreSQL default is ReadCommitted; we set it explicitly for clarity.
            // Use RepeatableRead/Serializable only when a use case needs stronger guarantees
            // than unique constraints + retry (not required for current chat flows).
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                // all pending inserts commit together in this explicit transaction

                await _unitOfWork.SaveChangesAsync(ct);

                await _unitOfWork.CommitTransactionAsync(ct);
                return session.Adapt<ChatSessionResponse>();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                // Unique index lost the race → return the row the other request created
                var raced = await _memberRepository.FindDirectSessionAsync(
                    initiatorId, participantId, organizationId, ct)
                    ?? throw new InvalidOperationException(
                       "Failed to create or find direct chat session after a concurrency conflict.");

                return raced.Adapt<ChatSessionResponse>();
            }
        }
    }
}
