using System.ComponentModel.DataAnnotations;

namespace Vention.Application
{

    public sealed class GatewayOptions
    {
        public const string SectionName = "Gateway";

        [Required(AllowEmptyStrings = false), MinLength(16)]
        public string SharedSecret { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string UserIdHeaderName { get; set; } = "X-User-Id";

        [Required(AllowEmptyStrings = false)]
        public string GatewaySecretHeaderName { get; set; } = "X-Gateway-Secret";
    }
}