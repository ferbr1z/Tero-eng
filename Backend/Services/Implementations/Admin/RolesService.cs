using MbyteWeb.Server.Authorization;
using MbyteWeb.Server.Exceptions;
using MbyteWeb.Server.Helpers;
using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.ViewModels.Admin.Usuarios;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using MbyteWeb.Server.Services.Interfaces.Auth;
using MbyteWeb.Server.ViewModels.Admin.Roles;

namespace MbyteWeb.Server.Services.Implementations.Admin
{
    public class RolesService(
        UserManager<Usuario> userManager,
        RoleManager<Rol> roleManager) : IRolesService
        
    {
        public async Task<Rol> Post(RolAddRequest rolAdd)
        {
            var rol = new Rol
            {
                Name = rolAdd.Nombre,
            };
            
            if (await roleManager.RoleExistsAsync(rol.Name))
            {
                throw new BadRequestException($"El rol '{rol.Name}' ya existe.");
            }
            
            //crear rol
            var result = await roleManager.CreateAsync(rol);
            if (!result.Succeeded)
            {
                throw new BadRequestException($"Error al crear el rol: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            
            return rol; // Retorna el rol creado
        }

        public async Task<Rol?> Get(int id)
        {
            var rol = await roleManager.FindByIdAsync(id.ToString());
            if (rol == null)
            {
                throw new NotFoundException($"Rol con ID '{id}' no encontrado.");
            }
            return rol;
        }
        
        public async Task<Rol?> GetByName(string name)
        {
            var rol = await roleManager.FindByNameAsync(name);
            if (rol == null)
            {
                throw new NotFoundException($"Rol con nombre '{name}' no encontrado.");
            }
            return rol;
        }

        public IQueryable<Rol> GetAll(RolesQuery query)
        {
            var filter = FilterBuilder.Create(roleManager.Roles);

            return filter.Contains(r => r.Name, query.Nombre)
                .Eq(r => r.Borrado, false)
                .ToQuery()
                .OrderByDescending(r => r.FechaCreacion);
        }

        public async Task<Rol> Put(Rol rol, RolEditRequest request)
        {
            // Prevenir modificar de admin y user
            if (rol.Name == RolesEnum.ADMIN_ROLE)
            {
                throw new BadRequestException("No se puede modificar el rol de administrador.");
            }
            if (rol.Name == RolesEnum.USER_ROLE)
            {
                throw new BadRequestException("No se puede modificar el rol de usuario.");
            }
            
            if (!string.IsNullOrEmpty(request.Nombre))
            {
                rol.Name = request.Nombre.Trim();
            }

            var result = await roleManager.UpdateAsync(rol);
            
            if (!result.Succeeded)
            {
                throw new BadRequestException($"Error al actualizar el rol: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return rol;
        }

        public async Task<Rol> Delete(Rol rol)
        {
            if (rol.Borrado) return rol;

            //prevenir borrar admin y user
            if (rol.Name == RolesEnum.ADMIN_ROLE)
                throw new BadRequestException("No se puede borrar el rol de administrador.");
    
            if (rol.Name == RolesEnum.USER_ROLE)
                throw new BadRequestException("No se puede borrar el rol de usuario.");

            var originalName = rol.Name;
            
            var usersInRole = await userManager.GetUsersInRoleAsync(originalName!);

            // eliminar el rol de los usuarios de ese rol
            foreach (var user in usersInRole)
            {
                var removeResult = await userManager.RemoveFromRoleAsync(user, originalName!);
                if (!removeResult.Succeeded)
                {
                    throw new BadRequestException($"Error al eliminar el rol de los usuarios: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                }
            }
            
            rol.Name = $"{rol.Name}_{rol.Id}";
            rol.Borrado = true;

            var result = await roleManager.UpdateAsync(rol);
            if (!result.Succeeded)
            {
                throw new BadRequestException("No se ha podido borrar el rol.");
            }

            return rol;
        }


        public async Task<bool> IsRolUsed(int id)
        {
            var rol = await Get(id);
            if (rol == null) return false;

            // Check if the role is assigned to any user
            var usersInRole = await userManager.GetUsersInRoleAsync(rol.Name!);
            return usersInRole.Any();
        }
        
        public async Task<bool> IsRolAdmin(Rol rol)
        {
            if (rol == null) return false;

            // Check if the role is admin
            return rol.Name == RolesEnum.ADMIN_ROLE;
        }
    }
}
