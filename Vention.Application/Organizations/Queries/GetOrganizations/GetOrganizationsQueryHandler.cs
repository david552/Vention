using Mapster;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Organizations.Queries.GetOrganizations
{
    public sealed class GetOrganizationsQueryHandler : IQueryHandler<GetOrganizationsQuery, IReadOnlyList<OrganizationResponse>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMembershipRepository _membershipRepository;
        public GetOrganizationsQueryHandler(
            IOrganizationRepository organizationRepository,
            IMembershipRepository membershipRepository)
        {
            _organizationRepository = organizationRepository;
            _membershipRepository = membershipRepository;
        }
        public async Task<IReadOnlyList<OrganizationResponse>> Handle(
            GetOrganizationsQuery query,
            CancellationToken ct)
        {
            var memberships = await _membershipRepository.GetByUserIdAsync(
                new UserId(query.ActingUserId), ct);

            if (memberships.Count == 0)
                return Array.Empty<OrganizationResponse>();

            var organizations = new List<OrganizationResponse>(memberships.Count);

            foreach (var membership in memberships)
            {
                var organization = await _organizationRepository.GetByIdAsync(
                    membership.OrganizationId, ct);

                if (organization is null)
                    continue;

                organizations.Add(organization.Adapt<OrganizationResponse>());
            }
            return organizations;
        }
    }
}
