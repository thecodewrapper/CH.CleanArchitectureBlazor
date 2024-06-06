using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Core.Application.DTOs;
using CH.CleanArchitecture.Core.Application.ReadModels;

namespace CH.CleanArchitecture.Presentation.WebApp.Models
{
    internal class UserDetailsFormModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Phone]
        [Display(Name = "Secondary Phone number")]
        public string SecondaryPhoneNumber { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string LanguagePreference { get; set; }

        public AddressDTO Address { get; set; } = new();
    }
}
