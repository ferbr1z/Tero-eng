using MbyteWeb.Server.Authorization;
using MbyteWeb.Server.Helpers;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.Services.Interfaces.Auth;
using MbyteWeb.Server.ViewModels.Admin.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace MbyteWeb.Server.Controllers
{
    [Route("api/usuarios")]
    [ApiController]
    [HasPermission(PermisosEnum.ManageUsuarios)]
    public class UsuariosController(IUsuariosService usuarios, IPermissionService permisos, IRolesService roles) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] UsuariosQuery query)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isCurrentUsuarioAdmin = await usuarios.IsCurrentUsuarioAdmin();
            var result = usuarios.GetAll(query, isCurrentUsuarioAdmin);

            //Paginate
            var response = await QueryResponseBuilder.Create(result, query.Page, query.PageSize, Mapper.ParseToDto);

            return Ok(response);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await GetVisible(id);
            if (usuario == null) return NotFound();
            return Ok(Mapper.ParseToDto(usuario));
        }
        
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] UsuarioAddRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await usuarios.Post(request);
            var assignPermissionsFromRoles = true;
            //si el request tiene roles y la configuración está habilitada, asignar permisos
            if (assignPermissionsFromRoles && request.Roles != null && request.Roles.Any())
            {
                //evitar acceso a la base de datos por cada rol, juntar todos los permisos de los roles
                var allPermissions = new HashSet<string>();
                foreach (var rol in request.Roles)
                {
                    var nRol = await roles.GetByName(rol);
                    if (nRol != null)
                    {
                        var rolPermisos = await permisos.GetPermissionsByRol(nRol);
                        foreach (var permiso in rolPermisos)
                        {
                            allPermissions.Add(permiso);
                        }
                    }
                }
                
                if (allPermissions.Any())
                {
                    await permisos.SetPermissionsToUsuarioWithList(usuario, allPermissions);
                }
            }
            
            return Ok(Mapper.ParseToDto(usuario));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] UsuarioEditRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await GetVisible(id);
            if (usuario == null) return NotFound();
            usuario = await usuarios.Put(usuario, request);
            var assignPermissionsFromRoles = true;
            // si hay roles y congiruracion
            if (assignPermissionsFromRoles && request.Roles != null && request.Roles.Any())
            {
                //cargar todos los permisos de los roles
                var allRolePermissions = new HashSet<string>();
                foreach (var rol in request.Roles)
                {
                    var nRol = await roles.GetByName(rol);
                    if (nRol != null)
                    {
                        var rolPermisos = await permisos.GetPermissionsByRol(nRol);
                        foreach (var permiso in rolPermisos)
                        {
                            allRolePermissions.Add(permiso);
                        }
                    }
                }
                
                //Iniciar un nuevo diccionario con todos los permisos y establecerlos en false
                var allPermissions = PermisosEnum.GetPermisos();
                var permissionsToSet = allPermissions.ToDictionary(p => p, p => false);
                
                //Establecer los permisos de los roles en true
                foreach (var permiso in allRolePermissions)
                {
                    permissionsToSet[permiso] = true;
                }
                
                await permisos.SetPermissions(usuario, permissionsToSet);
            }
            
            return Ok(Mapper.ParseToDto(usuario));
        }

        [HttpPut("password/{id}")]
        public async Task<ActionResult> ResetPassword(int id, [FromBody] UsuarioSetPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await GetVisible(id);
            if (usuario == null) return NotFound();
            await usuarios.SetUserPassword(usuario, request.NewPassword, true);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await GetVisible(id);
            if (usuario == null) return NotFound();
            usuario = await usuarios.Delete(usuario);
            return Ok(Mapper.ParseToDto(usuario));
        }

        private async Task<Models.Admin.Usuario?> GetVisible(int id)
        {
            return await usuarios.Get(id);
        }
    }
}
