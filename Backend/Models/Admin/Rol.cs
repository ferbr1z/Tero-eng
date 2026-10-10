using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MbyteWeb.Server.Models.Admin
{
    public class Rol : IdentityRole<int>, ISoftDeletable, ITimeStamped
    {
        public List<Usuario> Usuarios { get; } = [];
        
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
