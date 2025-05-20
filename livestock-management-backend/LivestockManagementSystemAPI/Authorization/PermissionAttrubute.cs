using Microsoft.AspNetCore.Authorization;

namespace LivestockManagementSystemAPI.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission) : base($"{PermissionPolicyProvider.PERMISSION_POLICY_PREFIX}:{permission}")
        {
        }
    }
}
