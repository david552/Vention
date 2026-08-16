using System.Text.Json;
using Vention.GraphQL.Http.Exceptions;
using Vention.GraphQL.Http.Models;

namespace Vention.GraphQL.Http.Clients
{

    public sealed class VentionApiClient : IVentionApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _http;

        public VentionApiClient(HttpClient http) => _http = http;

        public Task<IReadOnlyList<UserDto>> GetUsersAsync(bool includeOrganisations, CancellationToken ct)
            => GetListAsync<UserDto>($"users?includeOrganisations={includeOrganisations}", ct);

        public Task<UserDto> GetUserAsync(Guid id, bool includeOrganisations, CancellationToken ct)
            => GetAsync<UserDto>($"users/{id}?includeOrganisations={includeOrganisations}", ct);

        public Task<UserDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken ct)
            => PostAsync<UserDto>("users", request, ct);

        public Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken ct)
            => SendJsonAsync<UserDto>(HttpMethod.Patch, $"users/{id}", request, ct);

        public Task<IReadOnlyList<OrganizationDto>> GetOrganizationsAsync(CancellationToken ct)
            => GetListAsync<OrganizationDto>("orgs", ct);

        public Task<OrganizationDto> GetOrganizationAsync(Guid id, CancellationToken ct)
            => GetAsync<OrganizationDto>($"orgs/{id}", ct);

        public Task<IReadOnlyList<OrganizationDto>> GetOrganizationsByIdsAsync(
            IReadOnlyList<Guid> ids,
            CancellationToken ct)
        {
            if (ids.Count == 0)
                return Task.FromResult<IReadOnlyList<OrganizationDto>>(Array.Empty<OrganizationDto>());

            var query = string.Join("&", ids.Distinct().Select(id => $"ids={id}"));
            return GetListAsync<OrganizationDto>($"orgs?{query}", ct);
        }

        public Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationRequestDto request, CancellationToken ct)
            => PostAsync<OrganizationDto>("orgs", request, ct);

        public Task<OrganizationDto> UpdateOrganizationAsync(Guid id, UpdateOrganizationRequestDto request, CancellationToken ct)
            => SendJsonAsync<OrganizationDto>(HttpMethod.Patch, $"orgs/{id}", request, ct);

        public Task<IReadOnlyList<MembershipDto>> GetMembershipsByUserIdsAsync(
            IReadOnlyList<Guid> userIds,
            CancellationToken ct)
        {
            if (userIds.Count == 0)
                return Task.FromResult<IReadOnlyList<MembershipDto>>(Array.Empty<MembershipDto>());

            var query = string.Join("&", userIds.Distinct().Select(id => $"userIds={id}"));
            return GetListAsync<MembershipDto>($"memberships?{query}", ct);
        }

        private async Task<T> GetAsync<T>(string url, CancellationToken ct)
        {
            var response = await _http.GetAsync(url, ct);
            await EnsureSuccess(response, ct);
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct)
                ?? throw new RestApiException((int)response.StatusCode, "Empty JSON body.");
        }

        private async Task<IReadOnlyList<T>> GetListAsync<T>(string url, CancellationToken ct)
        {
            var response = await _http.GetAsync(url, ct);
            await EnsureSuccess(response, ct);
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<T>>(JsonOptions, ct)
                ?? Array.Empty<T>();
        }

        private Task<T> PostAsync<T>(string url, object body, CancellationToken ct)
            => SendJsonAsync<T>(HttpMethod.Post, url, body, ct);

        private async Task<T> SendJsonAsync<T>(HttpMethod method, string url, object body, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };

            var response = await _http.SendAsync(request, ct);
            await EnsureSuccess(response, ct);
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct)
                ?? throw new RestApiException((int)response.StatusCode, "Empty JSON body.");
        }

        private static async Task EnsureSuccess(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body = await response.Content.ReadAsStringAsync(ct);
            var code = (int)response.StatusCode switch
            {
                400 => "VALIDATION_ERROR",
                401 => "UNAUTHORIZED",
                403 => "FORBIDDEN",
                404 => "NOT_FOUND",
                _ => "REST_ERROR"
            };

            throw new RestApiException((int)response.StatusCode, string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase ?? code : body, code);
        }
    }
}