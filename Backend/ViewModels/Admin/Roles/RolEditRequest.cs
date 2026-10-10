using System.ComponentModel.DataAnnotations;

namespace MbyteWeb.Server.ViewModels.Admin.Roles
{
    public class RolEditRequest
    {
        [Required]
        [StringLength(100, ErrorMessage = "El nombre del rol debe tener entre 1 y 100 caracteres.", MinimumLength = 1)]
        public required string Nombre { get; set; }
        
        public required IDictionary<string, bool>? Permisos { get; set; } //opcional al editar un rol

    }
}