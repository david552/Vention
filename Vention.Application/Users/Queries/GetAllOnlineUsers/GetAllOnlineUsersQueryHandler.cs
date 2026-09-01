using Vention.Application.Abstractions;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Membership;
using Vention.Domain.Users;

namespace Vention.Application.Users.Queries.GetAllOnlineUsers
{

    public sealed class GetAllOnlineUsersQueryHandler
        : IQueryHandler<GetAllOnlineUsersQuery, IReadOnlyList<OnlineUserResponse>>
    {
        private readonly IPresenceTracker _presenceTracker;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserRepository _userRepository;

        public GetAllOnlineUsersQueryHandler(
            IPresenceTracker presenceTracker,
            IMembershipRepository membershipRepository,
            IUserRepository userRepository)
        {
            _presenceTracker = presenceTracker;
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
        }

        public async Task<IReadOnlyList<OnlineUserResponse>> Handle(
            GetAllOnlineUsersQuery query,
            CancellationToken ct)
        {
            var visibleUserIds = await GetVisibleUserIdsAsync(query.ActingUserId, ct);

            var onlineUserIds = await _presenceTracker.GetAllOnlineUsersAsync();

            var onlineVisibleIds = onlineUserIds
                .Select(id => Guid.TryParse(id, out var parsed) ? parsed : (Guid?)null)
                .Where(id => id.HasValue && visibleUserIds.Contains(id.Value))
                .Select(id => id!.Value)
                .ToArray();

            if (onlineVisibleIds.Length == 0)
                return Array.Empty<OnlineUserResponse>();

            var users = await _userRepository.GetByIdsAsync(
                onlineVisibleIds.Select(id => new UserId(id)).ToArray(),
                ct);

            return users
                .Select(user => new OnlineUserResponse(user.Id.Value, user.Name, user.Email.Value))
                .OrderBy(user => user.Name)
                .ToList();
        }

        private async Task<HashSet<Guid>> GetVisibleUserIdsAsync(Guid actingUserId, CancellationToken ct)
        {
            var visibleUserIds = new HashSet<Guid> { actingUserId };

            var actingMemberships = await _membershipRepository.GetByUserIdAsync(
                new UserId(actingUserId),
                ct);

            foreach (var membership in actingMemberships)
            {
                var orgMembers = await _membershipRepository.GetByOrganizationIdAsync(
                    membership.OrganizationId,
                    ct);

                foreach (var orgMember in orgMembers)
                    visibleUserIds.Add(orgMember.UserId.Value);
            }

            var isOwnerOrAdminAnywhere = actingMemberships
                .Any(m => MembershipRoleRules.IsOwnerOrAdmin(m.Role));

            if (isOwnerOrAdminAnywhere)
            {
                var orphans = await _userRepository.GetUsersWithNoMembershipsAsync(ct);

                foreach (var orphan in orphans)
                    visibleUserIds.Add(orphan.Id.Value);
            }

            return visibleUserIds;
        }
    }
}