using Vention.GraphQL.Http.Clients;
using Vention.GraphQL.Http.Models;
using Vention.GraphQL.Types.Inputs;

namespace Vention.GraphQL.Mutations
{

    public sealed class MutationRoot
    {
        public Task<UserDto> CreateUser(
            CreateUserInput input,
            [Service] IVentionApiClient api,
            CancellationToken ct)
            => api.CreateUserAsync(new CreateUserRequestDto
            {
                Email = input.Email,
                DisplayName = input.Name,
                Password = input.Password
            }, ct);

        public Task<UserDto> UpdateUser(
            Guid id,
            UpdateUserInput input,
            [Service] IVentionApiClient api,
            CancellationToken ct)
            => api.UpdateUserAsync(id, new UpdateUserRequestDto { DisplayName = input.DisplayName }, ct);

        public Task<OrganizationDto> CreateOrganization(
            CreateOrganizationInput input,
            [Service] IVentionApiClient api,
            CancellationToken ct)
            => api.CreateOrganizationAsync(new CreateOrganizationRequestDto { Name = input.Name }, ct);

        public Task<OrganizationDto> UpdateOrganization(
            Guid id,
            UpdateOrganizationInput input,
            [Service] IVentionApiClient api,
            CancellationToken ct)
            => api.UpdateOrganizationAsync(id, new UpdateOrganizationRequestDto { Name = input.Name }, ct);
    }
}