using System.ComponentModel.DataAnnotations;

namespace Vention.Application.Options
{

    public sealed class SignalRRedisOptions
    {
        public const string SectionName = "SignalR:Redis";

        [Required(AllowEmptyStrings = false)]
        public string ChannelPrefix { get; set; } = "Vention:SignalR:";

    }
}