using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.ViewModels.Admin.Usuarios;

namespace MbyteWeb.Server.Helpers;

public static class Mapper
{
    public static UsuarioDto ParseToDto(Usuario model) => new()
    {
        Id = model.Id,
        Nombre = model.Nombre,
        Email = model.Email,
        NumDocumento = model.NumDocumento,
        Telefono = model.Telefono,
        Direccion = model.Direccion,
        Roles = model.Roles.Select(rol => rol.Name).ToArray(),
        CreadoPor = model.CreadoPor,
        ModificadoPor = model.ModificadoPor,
        FechaCreacion = model.FechaCreacion,
        FechaModificacion = model.FechaModificacion
    };

    public static RolDto ParseToDto(Rol model) => new()
    {
        Id = model.Id,
        Nombre = model.Name,
        CreadoPor = model.CreadoPor,
        ModificadoPor = model.ModificadoPor,
        FechaCreacion = model.FechaCreacion,
        FechaModificacion = model.FechaModificacion
    };
}
