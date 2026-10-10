namespace MbyteWeb.Server.ViewModels.Admin.RegistroAuditoria
{
    public class RegistroAuditoriasQuery : BaseQuery
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? TipoAuditoria { get; set; }
        public string? NombreUsuario { get; set; }
        public string? NombreTabla { get; set; }
        public string? ColumnasAfectadas { get; set; }
        public string? ClavePrimaria { get; set; }
        public string? ExcludeTablas { get; set; }
    }
}
