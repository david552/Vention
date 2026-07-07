using Mapster;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Organizations.Contracts;
using Vention.Domain.Organizations;

namespace Vention.Application.Organizations.Queries.GetOrganizationById
{
    public sealed class GetOrganizationByIdQueryHandler : IQueryHandler<GetOrganizationByIdQuery, OrganizationResponse>
    {
        private readonly IOrganizationRepository _organizationRepository;
        public GetOrganizationByIdQueryHandler(IOrganizationRepository organizationRepository) => _organizationRepository = organizationRepository;

        public async Task<OrganizationResponse> Handle(GetOrganizationByIdQuery query, CancellationToken ct)
        {
            var organization = await _organizationRepository.GetByIdAsync(new OrganizationId(query.Id), ct)
                ?? throw new NotFoundException($"Organization '{query.Id}' was not found.");

            return organization.Adapt<OrganizationResponse>();
        }
    }
}
