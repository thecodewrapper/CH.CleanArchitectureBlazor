using System.ComponentModel.DataAnnotations;

namespace CH.CleanArchitecture.Presentation.WebApp.Models
{
    internal class ChangePasswordFormModel : IValidatableObject
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "The new password and confirmation password do not match.")]
        public string Password2 { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            if (OldPassword == Password) {
                yield return new ValidationResult("New password cannot be the same as the old password.", new[] { nameof(Password) });
            }
        }
    }
}
