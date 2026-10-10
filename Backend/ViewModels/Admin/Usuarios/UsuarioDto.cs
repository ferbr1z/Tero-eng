namespace MbyteWeb.Server.ViewModels.Admin.Usuarios
{
    public class UsuarioDto
    {
        public int? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? NumDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string?[] Roles { get; set; } = [];
        public string? CreadoPor { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
