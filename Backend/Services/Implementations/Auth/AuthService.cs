using MbyteWeb.Server.Exceptions;
using MbyteWeb.Server.Middlewares;
using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.Services.Interfaces.Auth;
using MbyteWeb.Server.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace MbyteWeb.Server.Services.Implementations.Auth
{
    public class AuthService(IUsuariosService usuarios, IPermissionService permissionService,
        SignInManager<Usuario> signInManager, IJwtService jwt, IConfiguration configuration) : IAuthService
    {
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var identifier = !string.IsNullOrWhiteSpace(request.Email)
                ? request.Email
                : request.Telefono!;
            var usuario = await usuarios.GetByIdentifier(identifier);
            if (usuario == null || usuario.Borrado)
            {
                throw new BadRequestException("Email o teléfono/Password inválido.");
            }

            var passwordValid = await CheckPassword(usuario, request.Password);
            if (!passwordValid)
            {
                throw new BadRequestException("Email o teléfono/Password inválido.");
            }

            return await CreateSession(usuario);
        }

        public async Task<LoginResponse> CreateSession(Usuario usuario)
        {
            var permisos = await permissionService.GetPermissions(usuario);
            var token = jwt.GenerateToken([
                new (JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, usuario.Email??""),
                new (JwtRegisteredClaimNames.Name, usuario.Nombre),
                new (SessionVersionMiddleware.VersionClaim, usuario.SessionVersion.ToString()),
            ]);

            return new LoginResponse
            {
                Id = usuario.Id,
                Email = usuario.Email ?? "",
                Nombre = usuario.Nombre,
                Token = token,
                IsTemporal = usuario.IsPasswordTemporal,
                Permisos = permisos
            };
        }

        public async Task SetPassword(SetPasswordRequest request)
        {
            var usuario = await usuarios.GetCurrentUsuario();
            if (usuario == null) throw new BadRequestException();

            await usuarios.SetUserPassword(usuario, request.NewPassword, false);
        }

        private async Task<bool> CheckPassword(Usuario usuario, string password)
        {
            if (usuario.IsPasswordTemporal && DateTime.UtcNow >= usuario.PasswordValidUntil) return false;
            var result = await signInManager.CheckPasswordSignInAsync(usuario, password, false);
            return result.Succeeded;
        }

        public bool CheckAuthorizationPassword(string password)
        {
            var section = configuration.GetSection("Authentication");
            var realPassword = section.GetValue<string>("AuthorizationCodePassword");
            if (realPassword == null) return false;
            return realPassword == password;
        }
    }
}
