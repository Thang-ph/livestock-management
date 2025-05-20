using BusinessObjects;
using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BusinessObjects.ConfigModels;

namespace DataAccess.Repository.Services
{
    public class PermissionService : IPermissionRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly LmsContext _context;
        private readonly ILogger<PermissionService> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public PermissionService(RoleManager<IdentityRole> roleManager, LmsContext context, ILogger<PermissionService> logger, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<List<string>> GetPermissionsForRole(string roleId)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    _logger.LogError("[PermissionService.GetPermissionsForRole] - RoleId is null or empty");
                    throw new ArgumentNullException(nameof(roleId));
                }

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("[PermissionService.GetPermissionsForRole] - Role not found with ID {RoleId}", roleId);
                    return new List<string>();
                }

                var claims = await _roleManager.GetClaimsAsync(role);
                return claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
            }
            catch (Exception ex) when (ex is not ArgumentNullException)
            {
                _logger.LogError(ex, "[PermissionService.GetPermissionsForRole] - Error occurred while getting permissions for role with ID {RoleId}", roleId);
                throw;
            }
        }

        public async Task<bool> SetPermissionsForRole(string roleId, List<string> permissions)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    _logger.LogError("[PermissionService.SetPermissionsForRole] - RoleId is null or empty");
                    throw new ArgumentNullException(nameof(roleId));
                }

                if (permissions == null)
                {
                    _logger.LogError("[PermissionService.SetPermissionsForRole] - Permissions list is null");
                    throw new ArgumentNullException(nameof(permissions));
                }

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("[PermissionService.SetPermissionsForRole] - Role not found with ID {RoleId}", roleId);
                    return false;
                }

                // Get existing permission claims
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                var permissionClaims = existingClaims.Where(c => c.Type == "Permission").ToList();

                // Remove all existing permission claims
                foreach (var claim in permissionClaims)
                {
                    var result = await _roleManager.RemoveClaimAsync(role, claim);
                    if (!result.Succeeded)
                    {
                        _logger.LogError("[PermissionService.SetPermissionsForRole] - Failed to remove claim {ClaimValue} from role {RoleId}", claim.Value, roleId);
                        throw new InvalidOperationException($"Failed to remove claim {claim.Value} from role {roleId}");
                    }
                }

                // Add new permission claims
                foreach (var permission in permissions)
                {
                    var result = await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", permission));
                    if (!result.Succeeded)
                    {
                        _logger.LogError("[PermissionService.SetPermissionsForRole] - Failed to add permission {Permission} to role {RoleId}", permission, roleId);
                        throw new InvalidOperationException($"Failed to add permission {permission} to role {roleId}");
                    }
                }

                return true;
            }
            catch (Exception ex) when (ex is not ArgumentNullException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "[PermissionService.SetPermissionsForRole] - Error occurred while setting permissions for role {RoleId}", roleId);
                throw;
            }
        }

        public Task<Dictionary<string, List<string>>> GetAllPermissionsByModule()
        {
            try
            {
                _logger.LogInformation("[PermissionService.GetAllPermissionsByModule] - Getting all permissions by module");
                return Task.FromResult(Permissions.GetPermissionsByModule());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService.GetAllPermissionsByModule] - Error occurred while getting all permissions by module");
                throw;
            }
        }

        public Task<Dictionary<string, Dictionary<string, string>>> GetVietnamesePermissionsByModule()
        {
            try
            {
                return Task.FromResult(Permissions.GetVietnamesePermissionsByModule());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService.GetVietnamesePermissionsByModule] - Error occurred while getting all Vietnamese permissions by module");
                throw;
            }
        }

        public async Task<List<IdentityRole>> GetAllRoles()
        {
            try
            {
                _logger.LogInformation("[PermissionService.GetAllRoles] - Getting all roles");
                return await _roleManager.Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService.GetAllRoles] - Error occurred while getting all roles");
                throw;
            }
        }

        public async Task<IdentityRole> GetRoleById(string roleId)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    _logger.LogError("[PermissionService.GetRoleById] - RoleId is null or empty");
                    throw new ArgumentNullException(nameof(roleId));
                }

                _logger.LogInformation("[PermissionService.GetRoleById] - Getting role by ID: {RoleId}", roleId);
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("[PermissionService.GetRoleById] - Role not found with ID {RoleId}", roleId);
                }

                return role;
            }
            catch (Exception ex) when (ex is not ArgumentNullException)
            {
                _logger.LogError(ex, "[PermissionService.GetRoleById] - Error occurred while getting role with ID {RoleId}", roleId);
                throw;
            }
        }

        public async Task<IdentityRole> CreateRole(string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    _logger.LogError("[PermissionService.CreateRole] - RoleName is null or empty");
                    throw new ArgumentNullException(nameof(roleName));
                }

                // Check if role already exists
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogWarning("[PermissionService.CreateRole] - Role with name {RoleName} already exists", roleName);
                    return null;
                }

                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("[PermissionService.CreateRole] - Failed to create role {RoleName}. Errors: {Errors}", roleName, errors);
                    throw new InvalidOperationException($"Failed to create role {roleName}. Errors: {errors}");
                }

                // Tìm priority thấp nhất hiện tại và thiết lập priority cho role mới
                var roles = await GetAllRolesWithPriorityAsync();
                int lowestPriority = 1; // Mặc định nếu không có role nào

                if (roles.Any())
                {
                    lowestPriority = roles.Max(r => r.Priority == int.MaxValue ? 0 : r.Priority) + 1;
                }

                // Thêm claim priority mới
                await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Priority", lowestPriority.ToString()));

                _logger.LogInformation("[PermissionService.CreateRole] - Role created successfully with name: {RoleName} and priority: {Priority}",
                    roleName, lowestPriority);

                return role;
            }
            catch (Exception ex) when (ex is not ArgumentNullException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "[PermissionService.CreateRole] - Error occurred while creating role {RoleName}", roleName);
                throw;
            }
        }

        public async Task<IdentityRole> UpdateRole(string roleId, string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    _logger.LogError("[PermissionService.UpdateRole] - RoleId is null or empty");
                    throw new ArgumentNullException(nameof(roleId));
                }

                if (string.IsNullOrEmpty(roleName))
                {
                    _logger.LogError("[PermissionService.UpdateRole] - RoleName is null or empty");
                    throw new ArgumentNullException(nameof(roleName));
                }

                _logger.LogInformation("[PermissionService.UpdateRole] - Updating role {RoleId} to name {RoleName}", roleId, roleName);

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("[PermissionService.UpdateRole] - Role not found with ID {RoleId}", roleId);
                    return null;
                }

                // If name hasn't changed, return role
                if (role.Name == roleName)
                {
                    return role;
                }

                // Check if new name already exists
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogWarning("[PermissionService.UpdateRole] - Role with name {RoleName} already exists", roleName);
                    return null;
                }

                role.Name = roleName;
                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("[PermissionService.UpdateRole] - Failed to update role {RoleId} to {RoleName}. Errors: {Errors}", roleId, roleName, errors);
                    throw new InvalidOperationException($"Failed to update role {roleId} to {roleName}. Errors: {errors}");
                }

                _logger.LogInformation("[PermissionService.UpdateRole] - Successfully updated role {RoleId} to {RoleName}", roleId, roleName);
                return role;
            }
            catch (Exception ex) when (ex is not ArgumentNullException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "[PermissionService.UpdateRole] - Error occurred while updating role {RoleId} to {RoleName}", roleId, roleName);
                throw;
            }
        }

        public async Task<bool> DeleteRole(string roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return false;

                // Kiểm tra xem có user nào đang sử dụng role này không
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                if (usersInRole.Any())
                {
                    _logger.LogWarning("[PermissionService.DeleteRole] - Cannot delete role {RoleName} because it has {UserCount} users",
                        role.Name, usersInRole.Count);
                    throw new InvalidOperationException($"Không thể xóa vai trò này vì đang có {usersInRole.Count} tài khoản đang sử dụng. Vui lòng gán vai trò khác cho các tài khoản này trước khi xóa.");
                }

                var result = await _roleManager.DeleteAsync(role);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService.DeleteRole] - Error deleting role with ID: {RoleId}", roleId);
                throw;
            }
        }

        public async Task<int> GetRolePriorityAsync(string roleId)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    _logger.LogError("[PermissionService.GetRolePriorityAsync] - RoleId is null or empty");
                    throw new ArgumentNullException(nameof(roleId));
                }

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("[PermissionService.GetRolePriorityAsync] - Role not found with ID {RoleId}", roleId);
                    return int.MaxValue; // Default low priority
                }

                var claims = await _roleManager.GetClaimsAsync(role);
                var priorityClaim = claims.FirstOrDefault(c => c.Type == "Priority");

                if (priorityClaim != null && int.TryParse(priorityClaim.Value, out int priority))
                {
                    return priority;
                }

                return int.MaxValue; // Default low priority
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService.GetRolePriorityAsync] - Error while getting role priority for {RoleId}", roleId);
                throw;
            }
        }

        public async Task<int> GetRolePriorityByNameAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    _logger.LogError("[PermissionService.GetRolePriorityByNameAsync] - RoleName is null or empty");
                    throw new ArgumentNullException(nameof(roleName));
                }

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("[PermissionService.GetRolePriorityByNameAsync] - Role not found with name {RoleName}", roleName);
                    return int.MaxValue;
                }

                return await GetRolePriorityAsync(role.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService.GetRolePriorityByNameAsync] - Error while getting role priority for {RoleName}", roleName);
                throw;
            }
        }

        public async Task<ResponseModel> SetRolePriorityAsync(string roleId, int priority)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    _logger.LogError("[PermissionService.SetRolePriorityAsync] - RoleId is null or empty");
                    return new ResponseModel { IsSuccess = false, Message = "RoleId không được để trống" };
                }

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("[PermissionService.SetRolePriorityAsync] - Role not found with ID {RoleId}", roleId);
                    return new ResponseModel { IsSuccess = false, Message = "Không tìm thấy vai trò" };
                }

                // Kiểm tra và xóa claim priority cũ
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                var priorityClaim = existingClaims.FirstOrDefault(c => c.Type == "Priority");
                if (priorityClaim != null)
                {
                    await _roleManager.RemoveClaimAsync(role, priorityClaim);
                }

                // Thêm claim priority mới
                await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Priority", priority.ToString()));

                _logger.LogInformation("[PermissionService.SetRolePriorityAsync] - Priority {Priority} set for role {RoleName} ({RoleId})",
                    priority, role.Name, role.Id);

                return new ResponseModel { IsSuccess = true, Message = "Đã thiết lập mức ưu tiên cho vai trò thành công" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService.SetRolePriorityAsync] - Error while setting role priority for {RoleId}", roleId);
                return new ResponseModel { IsSuccess = false, Message = "Đã xảy ra lỗi khi thiết lập mức ưu tiên: " + ex.Message };
            }
        }

        public async Task<List<RoleWithPriority>> GetAllRolesWithPriorityAsync()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                var result = new List<RoleWithPriority>();

                foreach (var role in roles)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var priorityClaim = claims.FirstOrDefault(c => c.Type == "Priority");

                    int priority = int.MaxValue;
                    if (priorityClaim != null && int.TryParse(priorityClaim.Value, out int parsedPriority))
                    {
                        priority = parsedPriority;
                    }

                    result.Add(new RoleWithPriority
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Priority = priority
                    });
                }

                return result.OrderBy(r => r.Priority).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PermissionService.GetAllRolesWithPriorityAsync] - Error while getting all roles with priority");
                throw;
            }
        }


    }
}