using MbyteWeb.Server.Authorization;
using MbyteWeb.Server.Data;
using MbyteWeb.Server.Exceptions;
using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MbyteWeb.Server.Services.Implementations.Auth
{
    public class PermissionService(AppDbContext context, IUsuariosService usuarios, IRolesService roles) : IPermissionService
    {
        public async Task<IEnumerable<string>> GetPermissions(Usuario usuario)
        {
            if (await usuarios.IsUsuarioAdmin(usuario)) return PermisosEnum.GetPermisos();

            var permisos = await context.UsuarioPermisos
                .Where(up => up.UsuarioId == usuario.Id && up.HasPermiso)
                .Select(up => up.PermisoId)
                .ToListAsync();

            //Filter any invalid permission
            permisos = permisos.Where(IsPermissionValid).ToList();

            return permisos;
        }
        
        public async Task<IEnumerable<string>> GetPermissionsByRol(Rol rol)
        {
            if (await roles.IsRolAdmin(rol)) return PermisosEnum.GetPermisos();

            var permisos = await context.RolPermisos
                .Where(rp => rp.RolId == rol.Id && rp.HasPermiso)
                .Select(rp => rp.PermisoId)
                .ToListAsync();

            //Filter any invalid permission
            permisos = permisos.Where(IsPermissionValid).ToList();

            return permisos;
            
        }

        public async Task<bool> HasPermission(Usuario usuario, string permisoId)
        {
            if (!IsPermissionValid(permisoId)) return false;
            if (await usuarios.IsUsuarioAdmin(usuario)) return true;
            if (usuario.IsPasswordTemporal || usuario.Borrado) return false;

            var usuarioPermiso = await context.UsuarioPermisos
                .Where(up => up.UsuarioId == usuario.Id && up.PermisoId == permisoId)
                .FirstOrDefaultAsync();

            if (usuarioPermiso == null) return false;

            return usuarioPermiso.HasPermiso;
        }

        public async Task SetPermission(Usuario usuario, string permisoId, bool value)
        {
            if (await usuarios.IsUsuarioAdmin(usuario)) return;

            await SetPermissionAsync(usuario, permisoId, value);
            await context.SaveChangesAsync();
        }

        public async Task SetPermissions(Usuario usuario, IDictionary<string, bool> permisos)
        {
            if (await usuarios.IsUsuarioAdmin(usuario)) return;

            foreach (var (permisoId, value) in permisos)
            {
                await SetPermissionAsync(usuario, permisoId, value);
            }

            await context.SaveChangesAsync();

        }

        private async Task SetPermissionAsync(Usuario usuario, string permisoId, bool value)
        {
            var existingUsuarioPermiso = await context.UsuarioPermisos.FindAsync(usuario.Id, permisoId);

            if (existingUsuarioPermiso != null) //If it exist, just change the value and return;
            {
                existingUsuarioPermiso.HasPermiso = value;
                return;
            }

            //If value is false, there's no need to create, return
            if (!value) return;

            //Check if permiso ID is valid
            if (!IsPermissionValid(permisoId))
                throw new BadRequestException("El permiso no es válido");

            //Add if it is
            var newUsuarioPermiso = new UsuarioPermiso
            {
                UsuarioId = usuario.Id,
                PermisoId = permisoId,
                HasPermiso = value
            };

            await context.UsuarioPermisos.AddAsync(newUsuarioPermiso);
        }

        public async Task SetPermissionToRol(Rol rol, string permisoId, bool value)
        {
            if (await roles.IsRolAdmin(rol)) return;

            await SetPermissionToRolAsync(rol, permisoId, value);
            await context.SaveChangesAsync();
        }
        
        public async Task SetPermissionsToRol(Rol rol, IDictionary<string, bool> permisos)
        {
            if (await roles.IsRolAdmin(rol)) return;

            foreach (var (permisoId, value) in permisos)
            {
                await SetPermissionToRolAsync(rol, permisoId, value);
            }

            await context.SaveChangesAsync();

        }
        
        private async Task SetPermissionToRolAsync(Rol rol, string permisoId, bool value)
        {
            var existingRolPermiso = await context.RolPermisos.FindAsync(rol.Id, permisoId);

            if (existingRolPermiso != null) //If it exist, just change the value and return;
            {
                existingRolPermiso.HasPermiso = value;
                return;
            }

            //If value is false, there's no need to create, return
            if (!value) return;

            //Check if permiso ID is valid
            if (!IsPermissionValid(permisoId))
                throw new BadRequestException("El permiso no es válido");

            //Add if it is
            var newRolPermiso = new RolPermiso
            {
                RolId = rol.Id,
                PermisoId = permisoId,
                HasPermiso = value
            };

            await context.RolPermisos.AddAsync(newRolPermiso);
        }
        
        //agregar permisos sin una lista con bool, solo con string
        // [ "nombre_permiso1", "nombre_permiso2" ]
        public async Task SetPermissionsToUsuarioWithList(Usuario usuario, IEnumerable<string> permisos)
        {
            if (await usuarios.IsUsuarioAdmin(usuario)) return;

            foreach (var permisoId in permisos)
            {
                if (!IsPermissionValid(permisoId))
                    throw new BadRequestException("El permiso no es válido");

                await SetPermissionAsync(usuario, permisoId, true);
            }

            await context.SaveChangesAsync();
        }

        private bool IsPermissionValid(string permissionId)
        {
            return PermisosEnum.IsPermisoIdValid(permissionId);
        }
    }
}
