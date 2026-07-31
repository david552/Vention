using System.ComponentModel.DataAnnotations;

namespace Vention.Application.Options
{
    public class FileStorageSettingsOptions
    {

        [Required(AllowEmptyStrings = false)]
        public string RootPath { get; set; } = "Storage";
    }
}
