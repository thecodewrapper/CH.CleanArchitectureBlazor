using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Common.Attributes;

namespace CH.CleanArchitecture.Presentation.WebApp.Models
{
    public class UserFormModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Roles are required.")]
        [Display(Name = "Roles")]
        [EnsureOneElement(ErrorMessage = "At least one role must be selected.")]
        public List<string> Roles { get; set; } = new List<string>();
    }
}
