using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MbyteWeb.Server.Data;

public static class DefaultUsuarioSeeder
{
    private const string PhoneNumber = "0986433663";
    // Hash precomputado de la contraseña inicial "1234".
    private const string PasswordHash = "AQAAAAIAAYagAAAAEN502/EESYzc1W5Vsh3SnR1Lj9MxlI2LbWhvx0Ud9HYTwbMXybHxbrXC6vpzTTItgA==";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Rol>>();

        foreach (var roleName in new[] { RolesEnum.USER_ROLE, RolesEnum.ADMIN_ROLE })
        {
            if (await roleManager.RoleExistsAsync(roleName)) continue;
            var roleResult = await roleManager.CreateAsync(new Rol { Name = roleName });
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"No se pudo crear el rol {roleName}.");
        }

        var usuario = await userManager.FindByNameAsync(PhoneNumber);
        if (usuario is null)
        {
            usuario = new Usuario
            {
                UserName = PhoneNumber,
                PhoneNumber = PhoneNumber,
                Telefono = PhoneNumber,
                Nombre = "Fernando Brizuela",
                NumDocumento = "5095295",
                Direccion = string.Empty
            };

            usuario.PasswordHash = PasswordHash;
            var createResult = await userManager.CreateAsync(usuario);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo crear el usuario inicial: {string.Join(", ", createResult.Errors.Select(error => error.Description))}");
            }
        }

        var roles = await roleManager.Roles
            .Where(role => !role.Borrado && role.Name != null)
            .Select(role => role.Name!)
            .ToListAsync();
        var currentRoles = await userManager.GetRolesAsync(usuario);
        var rolesToAdd = roles.Except(currentRoles).ToArray();

        if (rolesToAdd.Length == 0) return;

        var addRolesResult = await userManager.AddToRolesAsync(usuario, rolesToAdd);
        if (!addRolesResult.Succeeded)
        {
            throw new InvalidOperationException($"No se pudieron asignar los roles al usuario inicial: {string.Join(", ", addRolesResult.Errors.Select(error => error.Description))}");
        }
    }
}
