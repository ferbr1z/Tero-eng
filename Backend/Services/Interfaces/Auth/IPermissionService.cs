using MbyteWeb.Server.Models.Admin;

namespace MbyteWeb.Server.Services.Interfaces.Auth
{
    public interface IPermissionService
    {
        public Task<IEnumerable<string>> GetPermissions(Usuario usuario);
        public Task<IEnumerable<string>> GetPermissionsByRol(Rol rol);
        public Task<bool> HasPermission(Usuario usuario, string permisoId);
        public Task SetPermission(Usuario usuario, string permisoId, bool value);
        public Task SetPermissions(Usuario usuario, IDictionary<string, bool> permisos);
        public Task SetPermissionsToRol(Rol rol, IDictionary<string, bool> permisos);
        public Task SetPermissionsToUsuarioWithList(Usuario usuario, IEnumerable<string> permisos); //usado al crear un usuario con rol
        public Task SetPermissionToRol(Rol rol, string permisoId, bool value);
    }
}
