using Vention.GraphQL.Http.Models;

namespace Vention.GraphQL.Http.Clients
{

    public interface IVentionApiClient
    {
        Task<IReadOnlyList<UserDto>> GetUsersAsync(bool includeOrganisations, CancellationToken ct);
        Task<UserDto> GetUserAsync(Guid id, bool includeOrganisations, CancellationToken ct);
        Task<UserDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken ct);
        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken ct);

        Task<IReadOnlyList<OrganizationDto>> GetOrganizationsAsync(CancellationToken ct);
        Task<OrganizationDto> GetOrganizationAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<OrganizationDto>> GetOrganizationsByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct);
        Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationRequestDto request, CancellationToken ct);
        Task<OrganizationDto> UpdateOrganizationAsync(Guid id, UpdateOrganizationRequestDto request, CancellationToken ct);

        Task<IReadOnlyList<MembershipDto>> GetMembershipsByUserIdsAsync(IReadOnlyList<Guid> userIds, CancellationToken ct);
    }
}