using Mapster;
using Vention.Application.Chats.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Chats;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Chats.Queries.GetSessionsForUser
{
    public sealed class GetSessionsForUserQueryHandler
        : IQueryHandler<GetSessionsForUserQuery, IReadOnlyList<ChatSessionResponse>>
    {
        private readonly IChatSessionMemberRepository _memberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;

        public GetSessionsForUserQueryHandler(
            IChatSessionMemberRepository memberRepository,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository)
        {
            _memberRepository = memberRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
        }

        public async Task<IReadOnlyList<ChatSessionResponse>> Handle(GetSessionsForUserQuery query, CancellationToken ct)
        {
            var userId = new UserId(query.UserId);
            var organizationId = new OrganizationId(query.OrganizationId);

            if (!await _userRepository.ExistsByIdAsync(userId, ct))
                throw new NotFoundException($"User '{query.UserId}' was not found.");

            if (!await _organizationRepository.ExistsByIdAsync(organizationId, ct))
                throw new NotFoundException($"Organization '{query.OrganizationId}' was not found.");

            var sessions = await _memberRepository.GetSessionsForUserAsync(userId, organizationId, ct);
            return sessions.Adapt<IReadOnlyList<ChatSessionResponse>>();
        }
    }
}
