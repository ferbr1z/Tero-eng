using MbyteWeb.Server.Authorization;
using MbyteWeb.Server.Helpers;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.Services.Interfaces.Auth;
using MbyteWeb.Server.ViewModels.Admin.Roles;

namespace MbyteWeb.Server.Controllers;
using Microsoft.AspNetCore.Mvc;

// Controllers/RolesController.cs
[Route("api/roles")]
[ApiController]
[HasPermission(PermisosEnum.ManageRoles)]
public class RolesController(IRolesService roles, IPermissionService permisos) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] RolesQuery query)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = roles.GetAll(query);

        //Paginate
        var response = await QueryResponseBuilder.Create(result, query.Page, query.PageSize, Mapper.ParseToDto);

        return Ok(response);

    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> Get(int id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var rol = await roles.Get(id);
        if (rol == null) return NotFound();
        return Ok(Mapper.ParseToDto(rol));
    }
    
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] RolAddRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var rol = await roles.Post(request);
        
        //si el request tiene permisos, asignarlos al rol
        if (request.Permisos != null)
        {
            await permisos.SetPermissionsToRol(rol, request.Permisos);
        }
        
        return Ok(Mapper.ParseToDto(rol));
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] RolEditRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var rol = await roles.Get(id);
        if (rol == null) return NotFound();
        rol = await roles.Put(rol, request);
        
        //si el request tiene permisos, asignarlos al rol
        if (request.Permisos != null)
        {
            await permisos.SetPermissionsToRol(rol, request.Permisos);
        }
        
        return Ok(Mapper.ParseToDto(rol));
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var rol = await roles.Get(id);
        if (rol == null) return NotFound();
        rol = await roles.Delete(rol);
        return Ok(Mapper.ParseToDto(rol));
    }
}
