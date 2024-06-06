using System.ComponentModel.DataAnnotations;

namespace CH.CleanArchitecture.Presentation.WebApp.Models
{
    internal class ApplicationConfigurationFormModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsEncrypted { get; set; }
    }
}
