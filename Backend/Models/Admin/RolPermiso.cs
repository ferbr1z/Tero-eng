using Microsoft.EntityFrameworkCore;

namespace MbyteWeb.Server.Models.Admin
{
    [PrimaryKey(nameof(RolId), nameof(PermisoId))]
    public class RolPermiso
    {
        public required int RolId { get; set; }
        public required string PermisoId { get; set; }
        public required bool HasPermiso { get; set; } = false;
        public Rol Rol { get; set; } = null!;
    }
}