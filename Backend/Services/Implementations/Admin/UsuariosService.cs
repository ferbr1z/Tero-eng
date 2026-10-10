using MbyteWeb.Server.Authorization;
using MbyteWeb.Server.Exceptions;
using MbyteWeb.Server.Helpers;
using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.ViewModels.Admin.Usuarios;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MbyteWeb.Server.Services.Implementations.Admin
{
    public class UsuariosService(
        UserManager<Usuario> userManager,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration) : IUsuariosService
    {
        public async Task<Usuario?> Get(int id)
        {
            var usuario = await userManager.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id && !u.Borrado);
            return usuario;
        }

        public IQueryable<Usuario> GetAll(UsuariosQuery query, bool includeAdmin = false)
        {
            var baseQuery = userManager.Users
                .Include(u => u.Roles);
            
            var filter = FilterBuilder.Create(baseQuery);

            if (!includeAdmin)
                filter.Where(u => u.Roles.Any(r => r.Name != RolesEnum.ADMIN_ROLE));

            if (query.RoleIds is { Count: > 0 } roleIds)
                filter.Where(u => u.Roles.Any(r => roleIds.Contains(r.Id)));

            return filter.Contains(u => u.Nombre, query.Nombre)
                .Eq(u => u.Borrado, false)
                .Contains(u => u.Email, query.Email)
                .Contains(u => u.Telefono, query.Telefono)
                .Contains(u => u.NumDocumento, query.NumDocumento)
                .Contains(u => u.Direccion, query.Direccion)
                .ToQuery()
                .OrderByDescending(u => u.FechaCreacion);
        }

        public async Task<Usuario?> GetByIdentifier(string identifier)
        {
            return await userManager.FindByNameAsync(NormalizeIdentifier(identifier));
        }

        public async Task<Usuario?> GetCurrentUsuario()
        {
            var usuarioId = GetCurrentUsuarioId();
            if (usuarioId == null) return null;
            var usuario = await userManager.FindByIdAsync(usuarioId.Value.ToString());
            return usuario;
        }

        public int? GetCurrentUsuarioId()
        {
            var usuarioId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(usuarioId, out var id) ? id : null;
        }

        public async Task<bool> IsUsuarioAdmin(Usuario usuario)
        {
            return await userManager.IsInRoleAsync(usuario, RolesEnum.ADMIN_ROLE);
        }

        public async Task<bool> IsUsuarioAdmin(int id)
        {
            var usuario = await userManager.FindByIdAsync(id.ToString());
            if (usuario == null) throw new KeyNotFoundException();
            return await IsUsuarioAdmin(usuario);
        }

        public async Task<bool> IsCurrentUsuarioAdmin()
        {
            var currentUsuarioId = GetCurrentUsuarioId();
            if (currentUsuarioId == null) return false;
            return await IsUsuarioAdmin(currentUsuarioId.Value);
        }

        public async Task<Usuario> Post(UsuarioAddRequest request, bool temporalPassword = false)
        {
            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            var telefono = request.Telefono?.Trim() ?? string.Empty;
            var userName = email ?? NormalizePhoneNumber(telefono);

            var usuario = new Usuario
            {
                Email = email,
                UserName = userName,
                Nombre = request.Nombre.Trim(),
                NumDocumento = request.NumDocumento.Trim(),
                Telefono = telefono,
                PhoneNumber = telefono,
                Direccion = request.Direccion.Trim(),
                PasswordValidUntil = temporalPassword ? GenerateNewPasswordExpirationDate() : null
            };
            
            //Create User
            var createUserResult = await userManager.CreateAsync(usuario, request.Password);
            if (!createUserResult.Succeeded)
            {
                throw new BadRequestException(HandleUserRegisterError(createUserResult.Errors));
            }

            //juntar todos los roles para asignar
            var rolesToAssign = new List<string>();
            if (request.Roles != null && request.Roles.Length > 0)
            {
                rolesToAssign.AddRange(request.Roles);
            }
            else
            {
                rolesToAssign.Add(RolesEnum.USER_ROLE);
            }

            
            // Assign all roles in a single operation
            var roleResult = await userManager.AddToRolesAsync(usuario, rolesToAssign);
            if (!roleResult.Succeeded) 
                throw new BadRequestException(HandleUserRegisterError(roleResult.Errors));

            return usuario;
        }

        public async Task<Usuario> Put(Usuario usuario, UsuarioEditRequest request)
        {

            /*
             1- quitar roles actuales
             2- Agregar nuevos roles
            */
            var currentRoles = await userManager.GetRolesAsync(usuario);
            var removeRolesResult = await userManager.RemoveFromRolesAsync(usuario, currentRoles);
            if (!removeRolesResult.Succeeded)
                throw new BadRequestException(HandleUserRegisterError(removeRolesResult.Errors));

            var rolesToAssign = new List<string>();
            if (request.Roles != null && request.Roles.Length > 0)
                rolesToAssign.AddRange(request.Roles);
            else
                rolesToAssign.Add(RolesEnum.USER_ROLE);

            var addRolesResult = await userManager.AddToRolesAsync(usuario, rolesToAssign);
            if (!addRolesResult.Succeeded)
                throw new BadRequestException(HandleUserRegisterError(addRolesResult.Errors));

            // datos
            if (!string.IsNullOrEmpty(request.Nombre))
                usuario.Nombre = request.Nombre.Trim();
            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            var telefono = request.Telefono?.Trim() ?? string.Empty;
            usuario.Email = email;
            usuario.UserName = email ?? NormalizePhoneNumber(telefono);
            if (!string.IsNullOrEmpty(request.NumDocumento))
                usuario.NumDocumento = request.NumDocumento.Trim();
            usuario.Telefono = telefono;
            usuario.PhoneNumber = telefono;
            usuario.Direccion = request.Direccion?.Trim() ?? "";

            var result = await userManager.UpdateAsync(usuario);
            if (!result.Succeeded)
                throw new BadRequestException(HandleUserRegisterError(result.Errors));

            return usuario;
        }

        public async Task<Usuario> Delete(Usuario usuario)
        {
            if (await IsUsuarioAdmin(usuario)) throw new ForbiddenException();
            if (usuario.Borrado) return usuario;

            usuario.UserName = $"{usuario.UserName}_{usuario.Id}";
            usuario.NumDocumento = $"{usuario.NumDocumento}_{usuario.Id}";
            usuario.Borrado = true;

            var result = await userManager.UpdateAsync(usuario);
            if (!result.Succeeded) throw new BadRequestException("No se ha podido borrar el usuario.");
            return usuario;
        }

        public async Task SetUserPassword(Usuario usuario, string password, bool isTemporal = false)
        {
            if (!await CanManageThisUsuario(usuario)) throw new ForbiddenException();

            if (isTemporal) usuario.PasswordValidUntil = GenerateNewPasswordExpirationDate();
            else usuario.PasswordValidUntil = null;

            var token = await userManager.GeneratePasswordResetTokenAsync(usuario);
            var result = await userManager.ResetPasswordAsync(usuario, token, password);

            if (!result.Succeeded) throw new BadRequestException(HandleUserRegisterError(result.Errors));
        }

        private async Task<bool> CanManageThisUsuario(Usuario usuario)
        {
            return await IsCurrentUsuarioAdmin() || !await IsUsuarioAdmin(usuario);
        }

        private static string HandleUserRegisterError(IEnumerable<IdentityError> errors)
        {
            var identityError = errors.First();

            return identityError.Code switch
            {
                "DuplicateUserName" => "El email o teléfono ya existe.",
                "DuplicateEmail" => "El Email ya existe.",
                "InvalidEmail" => "El Email es inválido.",
                "InvalidUserName" => "El email o teléfono es inválido.",
                "PasswordTooShort" => "La contraseña es demasiado corta.",
                "PasswordRequiresDigit" => "La contraseña debe incluir al menos un número.",
                "PasswordRequiresLower" => "La contraseña debe incluir al menos una letra minúscula.",
                "PasswordRequiresUpper" => "La contraseña debe incluir al menos una letra mayúscula.",
                "PasswordRequiresNonAlphanumeric" => "La contraseña debe incluir al menos un carácter especial.",
                "PasswordRequiresUniqueChars" => "La contraseña debe tener más caracteres distintos.",
                _ => "No se ha podido guardar el usuario.",
            };
        }

        private DateTime GenerateNewPasswordExpirationDate()
        {
            var section = configuration.GetSection("Authentication");
            var hours = section.GetValue<int>("TemporalPasswordExpirationTime");
            if (hours <= 0) hours = 1;
            return DateTime.UtcNow.AddHours(hours);
        }

        private static string NormalizeIdentifier(string identifier)
        {
            var value = identifier.Trim();
            return value.Contains('@') ? value : NormalizePhoneNumber(value);
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }
    }
}
