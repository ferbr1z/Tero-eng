namespace MbyteWeb.Server.ViewModels
{
    public class BaseDto
    {
        public int? Id { get; set; }
        public string? CreadoPor { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool? Protegido { get; set; }
    }
}
