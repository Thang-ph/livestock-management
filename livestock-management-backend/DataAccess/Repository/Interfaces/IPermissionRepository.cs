using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Identity;

namespace DataAccess.Repository.Interfaces
{
    public interface IPermissionRepository
    {
        Task<List<string>> GetPermissionsForRole(string roleId);
        Task<bool> SetPermissionsForRole(string roleId, List<string> permissions);
        Task<Dictionary<string, List<string>>> GetAllPermissionsByModule();
        Task<Dictionary<string, Dictionary<string, string>>> GetVietnamesePermissionsByModule();
        Task<List<IdentityRole>> GetAllRoles();
        Task<IdentityRole> GetRoleById(string roleId);
        Task<IdentityRole> CreateRole(string roleName);
        Task<IdentityRole> UpdateRole(string roleId, string roleName);
        Task<bool> DeleteRole(string roleId);

        // Phương thức mới cho phân cấp role
        Task<int> GetRolePriorityAsync(string roleId);
        Task<int> GetRolePriorityByNameAsync(string roleName);
        Task<ResponseModel> SetRolePriorityAsync(string roleId, int priority);
        Task<List<RoleWithPriority>> GetAllRolesWithPriorityAsync();
    }
}
