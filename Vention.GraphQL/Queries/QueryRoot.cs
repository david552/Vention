using Vention.GraphQL.Http.Clients;
using Vention.GraphQL.Http.Models;

namespace Vention.GraphQL.Queries
{
    public sealed class QueryRoot
    {
        public Task<IReadOnlyList<UserDto>> GetUsers(
            [Service] IVentionApiClient api,
            CancellationToken ct)
            => api.GetUsersAsync(includeOrganisations: false, ct);

        public Task<UserDto> GetUser(
            Guid id,
            [Service] IVentionApiClient api,
            CancellationToken ct)
            => api.GetUserAsync(id, includeOrganisations: false, ct);

        public Task<IReadOnlyList<OrganizationDto>> GetOrganizations(
            [Service] IVentionApiClient api,
            CancellationToken ct)
            => api.GetOrganizationsAsync(ct);

        public Task<OrganizationDto> GetOrganization(
            Guid id,
            [Service] IVentionApiClient api,
            CancellationToken ct)
            => api.GetOrganizationAsync(id, ct);
    }
}