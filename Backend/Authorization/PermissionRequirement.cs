using Microsoft.AspNetCore.Authorization;

namespace MbyteWeb.Server.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionId { get; }

        public PermissionRequirement(string permissionId)
        {
            PermissionId = permissionId;
        }
    }
}
