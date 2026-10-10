namespace MbyteWeb.Server.ViewModels.Admin.Usuarios
{
    public class UsuariosQuery : BaseQuery
    {
        public string? Nombre { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? NumDocumento { get; set; } = string.Empty;
        public string? Telefono { get; set; } = string.Empty;
        public string? Direccion { get; set; } = string.Empty;
        public List<int>? RoleIds { get; set; }
    }
}
