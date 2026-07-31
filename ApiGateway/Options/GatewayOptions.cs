using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Options;

public sealed class GatewayOptions
{
    public const string SectionName = "Gateway";

    [Required, MinLength(16)]
    public string SharedSecret { get; set; } = string.Empty;
}