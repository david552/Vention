using Mapster;
using Vention.Application.Messaging;
using Vention.Application.Users.Contracts;
using Vention.Domain.Membership;
using Vention.Domain.Users;


namespace Vention.Application.Users.Queries.GetUsers
{
    public sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IReadOnlyList<UserResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly UserResponseComposer _composer;

        public GetUsersQueryHandler(
            IUserRepository userRepository,
            IMembershipRepository membershipRepository,
            UserResponseComposer composer)
        {
            _userRepository = userRepository;
            _membershipRepository = membershipRepository;
            _composer = composer;
        }

        public async Task<IReadOnlyList<UserResponse>> Handle(GetUsersQuery query, CancellationToken ct)
        {
            var actingUserId = new UserId(query.ActingUserId);
            var visibleUserIds = new HashSet<Guid> { query.ActingUserId };

            var actingMemberships = await _membershipRepository.GetByUserIdAsync(actingUserId, ct);

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

            var users = await _userRepository.GetByIdsAsync(
                visibleUserIds.Select(id => new UserId(id)).ToArray(),
                ct);

            if (!query.IncludeOrganisations)
            {
                return users
                    .Select(user => user.Adapt<UserResponse>() with
                    {
                        Organisations = Array.Empty<UserOrganizationMembershipResponse>()
                    })
                    .ToList();
            }

            return await _composer.ComposeManyAsync(users, ct);
        }
    }
}
