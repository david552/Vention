using Vention.Application.Exceptions;

namespace Vention.API.Extensions;

public static class HttpRequestTenantExtensions
{
    public const string OrgHeaderName = "x-org-id";

    public static Guid GetRequiredOrganizationId(this HttpRequest request)
    {
        var raw = request.Headers[OrgHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(raw))
            raw = request.Query["orgId"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var organizationId) || organizationId == Guid.Empty)

            throw new ArgumentException(
                $"Active organisation is required. Pass '{OrgHeaderName}' header or 'orgId' query parameter.",
                paramName: OrgHeaderName);

        return organizationId;
    }

    public static bool TryGetOrganizationId(this HttpRequest request, out Guid organizationId)
    {
        organizationId = Guid.Empty;
        var raw = request.Headers[OrgHeaderName].FirstOrDefault()
                  ?? request.Query["orgId"].FirstOrDefault();

        return !string.IsNullOrWhiteSpace(raw)
               && Guid.TryParse(raw, out organizationId)
               && organizationId != Guid.Empty;
    }
}