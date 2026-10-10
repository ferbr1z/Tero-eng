using System.Text.Json.Serialization;

namespace MbyteWeb.Server.Models.Admin
{
    public class RegistroAuditoria
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public TipoAuditoria TipoAuditoria { get; set; }
        public int? IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreTabla { get; set; } = string.Empty;
        public string ClavePrimaria { get; set; } = string.Empty;
        public string ColumnasAfectadas { get; set; } = string.Empty;
        public string ValoresAnteriores { get; set; } = string.Empty;
        public string ValoresNuevos { get; set; } = string.Empty;
        public int? StoreId { get; set; }

    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoAuditoria {
        Added,
        Modified,
        Deleted
    }
}
