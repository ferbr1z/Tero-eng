using MbyteWeb.Server.Models;
using MbyteWeb.Server.Models.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MbyteWeb.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : IdentityDbContext<Usuario, Rol, int>(options)
{
    public DbSet<RegistroAuditoria> RegistroAuditorias => Set<RegistroAuditoria>();
    public DbSet<UsuarioPermiso> UsuarioPermisos => Set<UsuarioPermiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();

    public bool SuppressAudit { get; set; }

    // Columnas de Usuario que se actualizan solas (Identity, timestamps, la propia versión de sesión):
    // cambiarlas no cuenta como "el usuario se modificó" ni para auditar ni para renovar su sesión.
    private static readonly HashSet<string> UsuarioBookkeepingProperties =
    [
        nameof(Usuario.ConcurrencyStamp), nameof(Usuario.SecurityStamp), nameof(Usuario.LockoutEnd),
        nameof(Usuario.AccessFailedCount), nameof(Usuario.FechaCreacion), nameof(Usuario.FechaModificacion),
        nameof(Usuario.ModificadorId), nameof(Usuario.ModificadoPor), nameof(Usuario.SessionVersion)
    ];

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        DefineModelStructure(builder);
    }

    private static void DefineModelStructure(ModelBuilder builder)
    {
        builder.Entity<Usuario>().HasMany(usuario => usuario.Roles).WithMany(rol => rol.Usuarios)
            .UsingEntity<IdentityUserRole<int>>(ur => ur.HasOne<Rol>().WithMany().HasForeignKey(x => x.RoleId), ur => ur.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UserId));

        builder.Entity<UsuarioPermiso>().HasOne(item => item.Usuario).WithMany().HasForeignKey(item => item.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<RolPermiso>().HasOne(item => item.Rol).WithMany().HasForeignKey(item => item.RolId).OnDelete(DeleteBehavior.Cascade);

    }

    public override int SaveChanges()
    {
        AddTimestamps();
        var usuarioIds = GetSessionChanges();
        PreventSessionVersionOverwrite();
        var pendingAudits = AddAuditRegistry();

        using var transaction = Database.CurrentTransaction == null ? Database.BeginTransaction() : null;
        var saved = base.SaveChanges();
        CompleteAuditKeys(pendingAudits);
        if (pendingAudits.Count > 0) base.SaveChanges();
        if (usuarioIds.Count > 0)
        {
            Users.Where(usuario => usuarioIds.Contains(usuario.Id))
                .ExecuteUpdate(setters => setters.SetProperty(usuario => usuario.SessionVersion, usuario => usuario.SessionVersion + 1));
            ReloadSessionVersions(usuarioIds);
        }
        transaction?.Commit();
        return saved;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        var usuarioIds = GetSessionChanges();
        PreventSessionVersionOverwrite();
        var pendingAudits = AddAuditRegistry();

        await using var transaction = Database.CurrentTransaction == null
            ? await Database.BeginTransactionAsync(cancellationToken)
            : null;
        var saved = await base.SaveChangesAsync(cancellationToken);
        CompleteAuditKeys(pendingAudits);
        if (pendingAudits.Count > 0) await base.SaveChangesAsync(cancellationToken);
        if (usuarioIds.Count > 0)
        {
            await Users.Where(usuario => usuarioIds.Contains(usuario.Id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(usuario => usuario.SessionVersion, usuario => usuario.SessionVersion + 1), cancellationToken);
            await ReloadSessionVersionsAsync(usuarioIds, cancellationToken);
        }
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        return saved;
    }

    /// <summary>
    /// Qué sesiones cambian con este SaveChanges: usuarios a los que se les modificaron datos,
    /// permisos o roles.
    /// </summary>
    private HashSet<int> GetSessionChanges()
    {
        var usuarioIds = new HashSet<int>();

        foreach (var entry in ChangeTracker.Entries())
        {
            int? usuarioId = (entry.Entity, entry.State) switch
            {
                (Usuario usuario, EntityState.Modified) when HasRealChanges(entry, property => !UsuarioBookkeepingProperties.Contains(property)) => usuario.Id,
                (UsuarioPermiso permiso, EntityState.Added or EntityState.Modified or EntityState.Deleted) => permiso.UsuarioId,
                (IdentityUserRole<int> usuarioRol, EntityState.Added or EntityState.Deleted) => usuarioRol.UserId,
                _ => null
            };
            if (usuarioId is int id) usuarioIds.Add(id);
        }

        return usuarioIds;
    }

    // Se comparan valores y no IsModified porque el UserStore de Identity hace Context.Update(user),
    // que marca todas las columnas como modificadas aunque no hayan cambiado.
    private static bool HasRealChanges(EntityEntry entry, Func<string, bool> isRelevant)
    {
        return entry.Properties.Any(property =>
            isRelevant(property.Metadata.Name)
            && !Equals(property.OriginalValue, property.CurrentValue));
    }

    private void PreventSessionVersionOverwrite()
    {
        // Identity.Update marca todas las columnas como modificadas. Una entidad leída antes de
        // otro incremento no debe volver a escribir una versión vieja durante base.SaveChanges.
        foreach (var entry in ChangeTracker.Entries<Usuario>().Where(entry => entry.State == EntityState.Modified))
            entry.Property(usuario => usuario.SessionVersion).IsModified = false;
    }

    // ExecuteUpdate evita perder incrementos concurrentes; se recarga el valor en entidades que
    // hayan quedado en el tracker para que un SaveChanges posterior no use una versión vieja.
    private void ReloadSessionVersions(HashSet<int> usuarioIds)
    {
        foreach (var entry in ChangeTracker.Entries<Usuario>().Where(entry => usuarioIds.Contains(entry.Entity.Id)).ToList())
            entry.Reload();
    }

    private async Task ReloadSessionVersionsAsync(HashSet<int> usuarioIds, CancellationToken cancellationToken)
    {
        foreach (var entry in ChangeTracker.Entries<Usuario>().Where(entry => usuarioIds.Contains(entry.Entity.Id)).ToList())
            await entry.ReloadAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(entry => entry.Entity is ITimeStamped)
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

        var user = httpContextAccessor.HttpContext?.User;
        var userId = int.TryParse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ? parsedUserId : (int?)null;
        var userName = user?.FindFirst(JwtRegisteredClaimNames.Name)?.Value ?? "Anonymous User";

        foreach (var entry in entries)
        {
            var model = (ITimeStamped)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                model.FechaCreacion = DateTime.UtcNow;
                model.CreadorId = userId;
                model.CreadoPor = userName;
            }
            else
            {
                model.FechaModificacion = DateTime.UtcNow;
                model.ModificadorId = userId;
                model.ModificadoPor = userName;
            }
        }
    }

    private List<(EntityEntry Entry, RegistroAuditoria Audit)> AddAuditRegistry()
    {
        var pendingAudits = new List<(EntityEntry Entry, RegistroAuditoria Audit)>();
        if (SuppressAudit)
        {
            return pendingAudits;
        }

        var entries = ChangeTracker.Entries()
            .Where(entry => entry.Entity is not RegistroAuditoria)
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var user = httpContextAccessor.HttpContext?.User;
        var userId = int.TryParse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ? parsedUserId : (int?)null;
        var userName = user?.FindFirst(JwtRegisteredClaimNames.Name)?.Value ?? "Anonymous User";

        foreach (var group in entries.Where(entry => entry.Entity is UsuarioPermiso)
                     .GroupBy(entry => ((UsuarioPermiso)entry.Entity).UsuarioId))
        {
            var audit = CreatePermissionAuditEntry(userId, userName, "UsuarioPermisos", "UsuarioId", group.Key, group);
            if (audit is not null) RegistroAuditorias.Add(audit);
        }

        foreach (var group in entries.Where(entry => entry.Entity is RolPermiso)
                     .GroupBy(entry => ((RolPermiso)entry.Entity).RolId))
        {
            var audit = CreatePermissionAuditEntry(userId, userName, "RolPermisos", "RolId", group.Key, group);
            if (audit is not null) RegistroAuditorias.Add(audit);
        }

        foreach (var entry in entries.Where(entry => entry.Entity is not UsuarioPermiso and not RolPermiso))
        {
            if (ShouldSkipAuditEntry(entry)) continue;

            var audit = EntryToRegistroAuditoria(userId, userName, entry);
            if (audit is null) continue;

            RegistroAuditorias.Add(audit);
            if (entry.State == EntityState.Added && entry.Properties.Any(property => property.Metadata.IsPrimaryKey() && property.IsTemporary))
                pendingAudits.Add((entry, audit));
        }

        return pendingAudits;
    }

    private static void CompleteAuditKeys(List<(EntityEntry Entry, RegistroAuditoria Audit)> pendingAudits)
    {
        foreach (var (entry, audit) in pendingAudits)
        {
            audit.ClavePrimaria = JsonSerializer.Serialize(entry.Properties
                .Where(property => property.Metadata.IsPrimaryKey())
                .ToDictionary(property => property.Metadata.Name, property => property.CurrentValue));
        }
    }

    private static bool ShouldSkipAuditEntry(EntityEntry entry)
    {
        return entry.Entity.GetType().Name == "IdentityUserRole`1";
    }

    private static RegistroAuditoria? CreatePermissionAuditEntry(
        int? userId,
        string userName,
        string tableName,
        string ownerKeyName,
        int ownerId,
        IEnumerable<EntityEntry> entries)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var entry in entries)
        {
            var permissionId = entry.Entity switch
            {
                UsuarioPermiso permission => permission.PermisoId,
                RolPermiso permission => permission.PermisoId,
                _ => string.Empty
            };
            var property = entry.Property(nameof(UsuarioPermiso.HasPermiso));

            if (entry.State == EntityState.Added)
                newValues[$"Permiso_{permissionId}"] = property.CurrentValue;
            else if (entry.State == EntityState.Deleted)
                oldValues[$"Permiso_{permissionId}"] = property.OriginalValue;
            else if (property.IsModified)
            {
                oldValues[$"Permiso_{permissionId}"] = property.OriginalValue;
                newValues[$"Permiso_{permissionId}"] = property.CurrentValue;
            }
        }

        if (oldValues.Count == 0 && newValues.Count == 0) return null;

        return new RegistroAuditoria
        {
            Fecha = DateTime.UtcNow,
            IdUsuario = userId,
            NombreUsuario = userName,
            NombreTabla = tableName,
            TipoAuditoria = TipoAuditoria.Modified,
            ColumnasAfectadas = JsonSerializer.Serialize(new[] { nameof(UsuarioPermiso.HasPermiso) }),
            ClavePrimaria = JsonSerializer.Serialize(new Dictionary<string, object?> { [ownerKeyName] = ownerId }),
            ValoresAnteriores = JsonSerializer.Serialize(oldValues),
            ValoresNuevos = JsonSerializer.Serialize(newValues)
        };
    }

    private static RegistroAuditoria? EntryToRegistroAuditoria(int? userId, string userName, EntityEntry entry)
    {
        var auditType = entry.State switch
        {
            EntityState.Added => TipoAuditoria.Added,
            EntityState.Deleted => TipoAuditoria.Deleted,
            EntityState.Modified => TipoAuditoria.Modified,
            _ => (TipoAuditoria?)null
        };

        if (auditType is null) return null;

        var changedColumns = new List<string>();
        var keys = new Dictionary<string, object?>();
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
            {
                keys[propertyName] = property.IsTemporary ? 0 : property.CurrentValue ?? property.OriginalValue ?? 0;
                continue;
            }

            if (IsSensitive(propertyName))
            {
                if (auditType == TipoAuditoria.Modified && property.IsModified)
                    changedColumns.Add(propertyName);

                continue;
            }

            if (auditType == TipoAuditoria.Modified && property.IsModified)
                changedColumns.Add(propertyName);
            if (auditType is TipoAuditoria.Deleted or TipoAuditoria.Modified)
                oldValues[propertyName] = property.OriginalValue;
            if (auditType is TipoAuditoria.Added or TipoAuditoria.Modified)
                newValues[propertyName] = property.CurrentValue;
        }

        if (entry.Entity is Usuario && auditType == TipoAuditoria.Modified)
        {
            if (changedColumns.All(UsuarioBookkeepingProperties.Contains)) return null;
        }

        return new RegistroAuditoria
        {
            Fecha = DateTime.UtcNow,
            IdUsuario = userId,
            NombreUsuario = userName,
            NombreTabla = entry.Entity.GetType().Name,
            TipoAuditoria = auditType.Value,
            ColumnasAfectadas = JsonSerializer.Serialize(changedColumns),
            ClavePrimaria = JsonSerializer.Serialize(keys),
            ValoresAnteriores = JsonSerializer.Serialize(oldValues),
            ValoresNuevos = JsonSerializer.Serialize(newValues)
        };
    }

    private static bool IsSensitive(string propertyName)
    {
        return propertyName is "PasswordHash" or "SecurityStamp" or "AccessToken" or "AuthenticatorKey";
    }
}
