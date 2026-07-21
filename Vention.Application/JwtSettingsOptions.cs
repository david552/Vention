using System.ComponentModel.DataAnnotations;

namespace Vention.Application
{
    public class JwtSettingsOptions
    {
        [Required(AllowEmptyStrings = false), MinLength(32)]
        public string Secret { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string Issuer { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int AccessTokenMinutes { get; set; } = 15;
    }
}