using Microsoft.AspNetCore.Authorization;

namespace MbyteWeb.Server.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        internal const string PolicyPrefix = "PERMISSION_";
        private const string Separator = "_";

        public HasPermissionAttribute(string permissionId)
        {
            Policy = PolicyPrefix + permissionId;
        }

        public static string GetPermissionId(string policyName) {
            return policyName.Split(Separator).Last();
        }
    }
}
