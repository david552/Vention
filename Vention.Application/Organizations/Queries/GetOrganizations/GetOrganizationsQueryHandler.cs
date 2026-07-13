using Mapster;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;
using Vention.Domain.Organizations;

namespace Vention.Application.Organizations.Queries.GetOrganizations
{
    public sealed class GetOrganizationsQueryHandler : IQueryHandler<GetOrganizationsQuery, IReadOnlyList<OrganizationResponse>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        public GetOrganizationsQueryHandler(IOrganizationRepository organizationRepository) => _organizationRepository = organizationRepository;

        public async Task<IReadOnlyList<OrganizationResponse>> Handle(GetOrganizationsQuery query, CancellationToken ct)
        {
            var organizations = await _organizationRepository.GetAllAsync(ct);

            return organizations.Adapt<IReadOnlyList<OrganizationResponse>>();

        }
    }
}
