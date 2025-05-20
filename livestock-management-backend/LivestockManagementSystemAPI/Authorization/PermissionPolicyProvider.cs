using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LivestockManagementSystemAPI.Authorization
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public const string PERMISSION_POLICY_PREFIX = "Permission";
        private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => BackupPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => BackupPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith($"{PERMISSION_POLICY_PREFIX}:", StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName.Substring(PERMISSION_POLICY_PREFIX.Length + 1);
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(permission));
                return Task.FromResult(policy.Build());
            }

            return BackupPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
