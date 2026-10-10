using System.Security.Claims;
using MbyteWeb.Server.Data;
using MbyteWeb.Server.Exceptions;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MbyteWeb.Server.Middlewares;

/// <summary>
/// Compara la versión de sesión del token con la del usuario en la DB. Si difiere (se le cambiaron
/// datos, permisos o roles), no se procesa el request: se responde 401 con el header
/// X-Session-Renewed y en el body las credenciales nuevas (el mismo JSON del login), para que el
/// frontend las guarde y repita el request.
/// </summary>
public class SessionVersionMiddleware(RequestDelegate next)
{
    public const string VersionClaim = "session_version";
    public const string RenewedHeader = "X-Session-Renewed";

    public async Task InvokeAsync(HttpContext httpContext, AppDbContext context, IUsuariosService usuarios, IAuthService auth)
    {
        var hasUsuarioId = int.TryParse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var usuarioId);
        if (!hasUsuarioId || !RequiresAuthorization(httpContext))
        {
            await next(httpContext);
            return;
        }

        var currentVersion = await context.Users
            .Where(usuario => usuario.Id == usuarioId && !usuario.Borrado)
            .Select(usuario => (int?)usuario.SessionVersion)
            .FirstOrDefaultAsync();
        if (currentVersion == null) throw new UnauthorizedException();

        if (httpContext.User.FindFirst(VersionClaim)?.Value == currentVersion.ToString())
        {
            await next(httpContext);
            return;
        }

        var usuario = await usuarios.Get(usuarioId) ?? throw new UnauthorizedException();
        var session = await auth.CreateSession(usuario);

        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.Headers[RenewedHeader] = "1";
        await httpContext.Response.WriteAsJsonAsync(session);
    }

    // Solo los endpoints que exigen login: así /auth/login y /auth/whoami no se interceptan aunque
    // llegue un token viejo.
    private static bool RequiresAuthorization(HttpContext httpContext)
    {
        var metadata = httpContext.GetEndpoint()?.Metadata;
        return metadata?.GetMetadata<IAuthorizeData>() != null && metadata.GetMetadata<IAllowAnonymous>() == null;
    }
}
