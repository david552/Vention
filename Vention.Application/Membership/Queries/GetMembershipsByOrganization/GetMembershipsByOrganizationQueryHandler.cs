using Mapster;
using Vention.Application.Membership.Contracts;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;

namespace Vention.Application.Membership.Queries.GetMembershipsByOrganization
{
    public sealed class GetMembershipsByOrganizationQueryHandler
        : IQueryHandler<GetMembershipsByOrganizationQuery, IReadOnlyList<MembershipResponse>>
    {
        private readonly IMembershipRepository _membershipRepository;
        public GetMembershipsByOrganizationQueryHandler(IMembershipRepository membershipRepository) => _membershipRepository = membershipRepository;

        public async Task<IReadOnlyList<MembershipResponse>> Handle(GetMembershipsByOrganizationQuery query, CancellationToken ct)
        {
            var memberships = await _membershipRepository.GetByOrganizationIdAsync(new OrganizationId(query.OrganizationId), ct);
            return memberships.Adapt<IReadOnlyList<MembershipResponse>>();
        }
    }
}
