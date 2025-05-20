using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LivestockManagementSystemAPI.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // If user has no identity, they're not authenticated
            if (!context.User.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            // Get all claims of type Permission
            var permissionClaims = context.User.Claims.Where(c => c.Type == "Permission").ToList();

            // Check if user has the required permission
            if (permissionClaims.Any(c => c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
