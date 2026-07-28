using System.Text.Json.Serialization;

namespace Vention.Application.Membership.Contracts
{
    public sealed record MembershipResponse(
        Guid Id,
        Guid UserId,
        [property: JsonPropertyName("organisationId")] Guid OrganisationId,
        [property: JsonPropertyName("type")] string Type);
}


