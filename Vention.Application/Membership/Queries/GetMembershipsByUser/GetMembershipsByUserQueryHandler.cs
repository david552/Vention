using Mapster;
using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Users;

namespace Vention.Application.Membership.Queries.GetMembershipsByUser
{
    public sealed class GetMembershipsByUserQueryHandler
       : IQueryHandler<GetMembershipsByUserQuery, IReadOnlyList<MembershipResponse>>
    {
        private readonly IMembershipRepository _membershipRepository;
        public GetMembershipsByUserQueryHandler(IMembershipRepository membershipRepository) => _membershipRepository = membershipRepository;

        public async Task<IReadOnlyList<MembershipResponse>> Handle(GetMembershipsByUserQuery query, CancellationToken ct)
        {
            var memberships = await _membershipRepository.GetByUserIdAsync(new UserId(query.UserId), ct);
            return memberships.Adapt<IReadOnlyList<MembershipResponse>>();
        }
    }
}
