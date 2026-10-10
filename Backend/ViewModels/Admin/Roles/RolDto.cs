
namespace MbyteWeb.Server.ViewModels.Admin.Usuarios
{
    public class RolDto
    {
        public int? Id { get; set; }
        public string? Nombre { get; set; }
        public string? CreadoPor { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
