using Microsoft.EntityFrameworkCore;

namespace MbyteWeb.Server.Models.Admin
{
    [PrimaryKey(nameof(UsuarioId), nameof(PermisoId))]
    public class UsuarioPermiso
    {
        public required int UsuarioId { get; set; }
        public required string PermisoId { get; set; }
        public required bool HasPermiso { get; set; } = false;
        public Usuario Usuario { get; set; } = null!;
    }
}
