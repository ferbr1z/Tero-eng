using System.Collections.Immutable;

namespace MbyteWeb.Server.Authorization;

public static class PermisosEnum
{
    public const string ManageUsuarios = "ManageUsuarios";
    public const string ManageRoles = "ManageRoles";
    public const string ManagePermisos = "ManagePermisos";
    public const string Auditoria = "Auditoria";
    private static readonly ImmutableHashSet<string> Permisos = [
        ManageUsuarios,
        ManageRoles,
        ManagePermisos,
        Auditoria
    ];
    public static IEnumerable<string> GetPermisos() => Permisos;
    public static bool IsPermisoIdValid(string permisoId) => Permisos.Contains(permisoId);
}
