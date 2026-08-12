using System.ComponentModel.DataAnnotations;

namespace Vention.Application.Options
{
    public sealed class RabbitMqSettingsOptions
    {
        public const string SectionName = "RabbitMqSettings";

        [Required(AllowEmptyStrings = false)]
        public string Host { get; set; } = "localhost";

        [Range(1, 65535)]
        public ushort Port { get; set; } = 5672;

        [Required(AllowEmptyStrings = false)]
        public string VirtualHost { get; set; } = "/";

        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; } = "guest";

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; } = "guest";
    }
}