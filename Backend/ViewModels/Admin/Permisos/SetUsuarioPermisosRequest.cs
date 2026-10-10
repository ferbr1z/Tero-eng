namespace MbyteWeb.Server.ViewModels.Admin.Permisos
{
    public class SetUsuarioPermisosRequest
    {
        public required IDictionary<string, bool> Permisos { get; set; }
    }
}
