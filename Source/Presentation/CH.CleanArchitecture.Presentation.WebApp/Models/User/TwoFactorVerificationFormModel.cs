using System.ComponentModel.DataAnnotations;

namespace CH.CleanArchitecture.Presentation.WebApp.Models
{
    internal class TwoFactorVerificationFormModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string Code { get; set; }
    }
}
