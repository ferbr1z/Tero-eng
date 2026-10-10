using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MbyteWeb.Server.Authorization
{
    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        { }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            //Get existing policy
            var existingPolicy = await base.GetPolicyAsync(policyName);

            //Check if it is custom policy
            if (!policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
                return existingPolicy;

            //If it already exists, return
            if (existingPolicy != null) 
                return existingPolicy;

            //Extract permission Id
            var permissionId = HasPermissionAttribute.GetPermissionId(policyName);

            //Return new one if it doesn't
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permissionId))
                .Build();
        }
    }
}
