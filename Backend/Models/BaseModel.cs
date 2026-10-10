using System.ComponentModel.DataAnnotations;

namespace MbyteWeb.Server.Models;

public abstract class BaseModel : IModel, ISoftDeletable, ITimeStamped
{
    public int Id { get; set; }

    public int? CreadorId { get; set; }
    public string? CreadoPor { get; set; }
    public int? ModificadorId { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public bool Borrado { get; set; }
    public bool Protegido { get; set; }

    [Timestamp]
    public uint Version { get; set; }
}
