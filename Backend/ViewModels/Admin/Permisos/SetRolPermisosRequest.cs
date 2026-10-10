namespace MbyteWeb.Server.ViewModels.Admin.Permisos
{
    public class SetRolPermisosRequest
    {
        public required IDictionary<string, bool> Permisos { get; set; }
    }
}
