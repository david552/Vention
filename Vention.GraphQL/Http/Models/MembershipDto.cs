using System.Text.Json.Serialization;

namespace Vention.GraphQL.Http.Models
{
    public sealed class MembershipDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }

        [JsonPropertyName("organisationId")]
        public Guid OrganisationId { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;
    }
}