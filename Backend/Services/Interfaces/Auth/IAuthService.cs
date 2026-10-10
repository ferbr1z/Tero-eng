using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.ViewModels.Auth;

namespace MbyteWeb.Server.Services.Interfaces.Auth
{
    public interface IAuthService
    {
        public Task<LoginResponse> Login(LoginRequest request);
        /// <summary>Credenciales de sesión (token + datos del usuario) tal cual las devuelve el login.</summary>
        public Task<LoginResponse> CreateSession(Usuario usuario);
        public Task SetPassword(SetPasswordRequest request);
        public bool CheckAuthorizationPassword(string password);
    }
}
