using Vention.Application.Abstractions;
using Vention.Application.Authorization;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Users.Queries.GetOnlineUsersByOrganization
{
    public sealed class GetOnlineUsersByOrganizationQueryHandler
        : IQueryHandler<GetOnlineUsersByOrganizationQuery, IReadOnlyList<OnlineUserResponse>>
    {
        private readonly IPresenceTracker _presenceTracker;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserRepository _userRepository;
        private readonly OrganizationAuthorizationService _organizationAuthorization;

        public GetOnlineUsersByOrganizationQueryHandler(
            IPresenceTracker presenceTracker,
            IMembershipRepository membershipRepository,
            IUserRepository userRepository,
            OrganizationAuthorizationService organizationAuthorization)
        {
            _presenceTracker = presenceTracker;
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _organizationAuthorization = organizationAuthorization;
        }

        public async Task<IReadOnlyList<OnlineUserResponse>> Handle(
            GetOnlineUsersByOrganizationQuery query,
            CancellationToken ct)
        {
            await _organizationAuthorization.EnsureIsOrganizationMemberAsync(
                query.ActingUserId,
                query.OrganizationId,
                ct);

            var groupName = PresenceGroups.ForOrganization(query.OrganizationId);
            var onlineUserIds = await _presenceTracker.GetOnlineUsersAsync(groupName);

            var orgMembers = await _membershipRepository.GetByOrganizationIdAsync(
                new OrganizationId(query.OrganizationId),
                ct);

            var orgMemberIds = orgMembers
                .Select(m => m.UserId.Value.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var onlineMemberIds = onlineUserIds
                .Where(orgMemberIds.Contains)
                .Select(Guid.Parse)
                .ToArray();

            if (onlineMemberIds.Length == 0)
                return Array.Empty<OnlineUserResponse>();

            var users = await _userRepository.GetByIdsAsync(
                onlineMemberIds.Select(id => new UserId(id)).ToArray(),
                ct);

            return users
                .Select(user => new OnlineUserResponse(user.Id.Value, user.Name, user.Email.Value))
                .OrderBy(user => user.Name)
                .ToList();
        }
    }
}