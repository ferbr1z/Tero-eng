using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.ViewModels.Admin.Roles;
using MbyteWeb.Server.ViewModels.Admin.Usuarios;

namespace MbyteWeb.Server.Services.Interfaces.Admin
{
    public interface IRolesService
    {
        public Task<Rol> Post(RolAddRequest rolAdd);
        public Task<Rol?> Get(int id);
        public Task<Rol?> GetByName(string name);
        public IQueryable<Rol> GetAll(RolesQuery query);
        public Task<Rol> Put(Rol rol, RolEditRequest request);
        public Task<Rol> Delete(Rol rol);
        public Task<bool> IsRolUsed(int id);
        public  Task<bool> IsRolAdmin(Rol rol);
    }
}
