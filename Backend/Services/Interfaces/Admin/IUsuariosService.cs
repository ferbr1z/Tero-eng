using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.ViewModels.Admin.Usuarios;

namespace MbyteWeb.Server.Services.Interfaces.Admin
{
    public interface IUsuariosService
    {
        public IQueryable<Usuario> GetAll(UsuariosQuery query, bool includeAdmin = false);
        public Task<Usuario?> Get(int id);
        public Task<Usuario?> GetByIdentifier(string identifier);
        public int? GetCurrentUsuarioId();
        public Task<Usuario?> GetCurrentUsuario();
        public Task<bool> IsUsuarioAdmin(Usuario usuario);
        public Task<bool> IsCurrentUsuarioAdmin();
        public Task<Usuario> Post(UsuarioAddRequest request, bool temporalPassword = false);
        public Task<Usuario> Put(Usuario usuario, UsuarioEditRequest request);
        public Task<Usuario> Delete(Usuario usuario);
        public Task SetUserPassword(Usuario usuario, string password, bool isTemporal = false);
    }
}
