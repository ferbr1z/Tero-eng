using System.ComponentModel.DataAnnotations;

namespace MbyteWeb.Server.ViewModels.Admin.Usuarios
{
    public class UsuarioAddRequest : IValidatableObject
    {
        [EmailAddress]
        public string? Email { get; set; }
        public required string Password { get; set; }
        public required string Nombre { get; set; }
        public required string NumDocumento { get; set; }
        
        public string[] Roles { get; set; } = [];
        public string? Telefono { get; set; }
        public string Direccion { get; set; } = string.Empty;

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
