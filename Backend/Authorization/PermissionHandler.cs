using MbyteWeb.Server.Services.Implementations;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MbyteWeb.Server.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public PermissionHandler(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            using var scope = serviceScopeFactory.CreateScope();

            var permissionService = scope.ServiceProvider.GetService<IPermissionService>();
            var usuariosService = scope.ServiceProvider.GetService<IUsuariosService>();
            if (permissionService == null || usuariosService == null) return;

            var usuarioId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioId, out var id)) return;

            var usuario = await usuariosService.Get(id);
            if(usuario == null) return;
            
            var permisoId = requirement.PermissionId;
            var hasPermiso = await permissionService.HasPermission(usuario, permisoId);

            if (hasPermiso)
            {
                context.Succeed(requirement);
            }
        }
    }
}
