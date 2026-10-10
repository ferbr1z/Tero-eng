using MbyteWeb.Server.Authorization;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.Services.Interfaces.Auth;
using MbyteWeb.Server.ViewModels.Admin.Permisos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MbyteWeb.Server.Controllers
{
    [Route("api/permisos")]
    [ApiController]
    [HasPermission(PermisosEnum.ManagePermisos)]
    public class PermisosController(IUsuariosService usuarios, IPermissionService permissionService, IRolesService roles) : ControllerBase
    {
        [HttpGet("catalogo")]
        public IActionResult GetCatalogo()
        {
            return Ok(PermisosEnum.GetPermisos());
        }

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> GetPermisos(int usuarioId) {
            var usuario = await usuarios.Get(usuarioId);
            if (usuario == null) return NotFound();
            var permisos = await permissionService.GetPermissions(usuario);
            return Ok(permisos);
        }
        
        [HttpGet("rol/{rolId}")]
        public async Task<IActionResult> GetPermisosByRol(int rolId) {
            var rol = await roles.Get(rolId);
            if (rol == null) return NotFound();
            var permisos = await permissionService.GetPermissionsByRol(rol);
            return Ok(permisos);
        }

        [HttpGet("rol/nombre/{nombre}")]
        public async Task<IActionResult> GetPermisosByRolNombre(string nombre) {
            var rol = await roles.GetByName(nombre);
            if (rol == null) return NotFound();
            return Ok(await permissionService.GetPermissionsByRol(rol));
        }

        [HttpPost("{usuarioId}")]
        public async Task<IActionResult> SetPermisos(int usuarioId, [FromBody] SetUsuarioPermisosRequest request) {
            var usuario = await usuarios.Get(usuarioId);
            if (usuario == null) return NotFound();
            await permissionService.SetPermissions(usuario, request.Permisos);
            return Ok();
        }
        
        [HttpPost("rol/{rolId}")]
        public async Task<IActionResult> SetPermisosByRol(int rolId, [FromBody] SetRolPermisosRequest request) {
            var rol = await roles.Get(rolId);
            if (rol == null) return NotFound();
            await permissionService.SetPermissionsToRol(rol, request.Permisos);
            return Ok();
        }
    }
}
