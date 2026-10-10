using System.ComponentModel.DataAnnotations;

namespace MbyteWeb.Server.ViewModels.Auth
{
    public class LoginRequest : IValidatableObject
    {
        [EmailAddress]
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        [Required]
        public required string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Telefono))
            {
                yield return new ValidationResult(
                    "Debe proporcionar un email o un teléfono.",
                    [nameof(Email), nameof(Telefono)]);
            }
        }
    }
}
