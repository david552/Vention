using Mapster;
using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Users;

namespace Vention.Application.Membership.Queries.GetMembershipsByUserIds
{

    public sealed class GetMembershipsByUserIdsQueryHandler
        : IQueryHandler<GetMembershipsByUserIdsQuery, IReadOnlyList<MembershipResponse>>
    {
        private readonly IMembershipRepository _memberships;

        public GetMembershipsByUserIdsQueryHandler(IMembershipRepository memberships)
            => _memberships = memberships;

        public async Task<IReadOnlyList<MembershipResponse>> Handle(
            GetMembershipsByUserIdsQuery query,
            CancellationToken ct)
        {
            if (query.UserIds.Count == 0)
                return Array.Empty<MembershipResponse>();

            var userIds = query.UserIds
                .Distinct()
                .Select(id => new UserId(id))
                .ToArray();

            var memberships = await _memberships.GetByUserIdsAsync(userIds, new UserId(query.ActingUserId), ct);
            return memberships.Adapt<IReadOnlyList<MembershipResponse>>();
        }
    }
}