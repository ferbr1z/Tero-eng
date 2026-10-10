using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MbyteWeb.Server.Models.Admin
{
    public class Usuario:IdentityUser<int>, ISoftDeletable, ITimeStamped
    {
        public required string Nombre { get; set; }
        public string NumDocumento { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        public List<Rol> Roles { get; } = [];

        //PASSWORD
        public bool IsPasswordTemporal { get => PasswordValidUntil != null; }
        public DateTime? PasswordValidUntil { get; set; }

        //SESSION
        // Sube +1 con cada cambio del usuario (datos, permisos, roles, tiendas). Viaja en el token y,
        // si no coincide con la DB, SessionVersionMiddleware devuelve una sesión renovada.
        public int SessionVersion { get; set; }

        //TIMESTAMPS
        public int? CreadorId { get; set; }
        [MaxLength(512)]
        public string? CreadoPor { get; set; }
        public int? ModificadorId { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Borrado { get; set; } = false;
        public bool Protegido { get; set; } = false;
    }
}
