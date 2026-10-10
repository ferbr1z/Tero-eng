namespace MbyteWeb.Server.Models
{
    public interface ITimeStamped
    {
        public int? CreadorId { get; set; }
        public string? CreadoPor { get; set; }
        public int? ModificadorId { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
