using System.ComponentModel.DataAnnotations;

namespace Vention.Application
{
    public class CryptoSettingsOptions
    {
        [Required(AllowEmptyStrings = false)]
        public string PasswordPepper { get; set; } = string.Empty;
    }
}
