using Mapster;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Organizations.Queries.GetOrganizationsByIds
{

    public sealed class GetOrganizationsByIdsQueryHandler
        : IQueryHandler<GetOrganizationsByIdsQuery, IReadOnlyList<OrganizationResponse>>
    {
        private readonly IOrganizationRepository _organizations;

        public GetOrganizationsByIdsQueryHandler(IOrganizationRepository organizations)
            => _organizations = organizations;

        public async Task<IReadOnlyList<OrganizationResponse>> Handle(
            GetOrganizationsByIdsQuery query,
            CancellationToken ct)
        {
            if (query.Ids.Count == 0)
                return Array.Empty<OrganizationResponse>();

            var ids = query.Ids
                .Distinct()
                .Select(id => new OrganizationId(id))
                .ToArray();

            var organizations = await _organizations.GetByIdsAsync(ids, new UserId(query.ActingUserId), ct);
            return organizations.Adapt<IReadOnlyList<OrganizationResponse>>();
        }
    }
}