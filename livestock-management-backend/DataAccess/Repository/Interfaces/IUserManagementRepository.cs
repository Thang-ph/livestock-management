using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Identity;

namespace DataAccess.Repository.Interfaces
{
    public interface IUserManagementRepository
    {
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<List<UserDTO>> SearchUsersAsync(string searchTerm, List<string> roles);
        Task<UserDTO> GetUserByIdAsync(string userId);
        Task<ResponseModel> CreateUserAsync(CreateUserDTO model);
        Task<ResponseModel> UpdateUserAsync(string userId, UpdateUserDTO model);
        Task<ResponseModel> BlockUserAsync(string userId, bool isBlocked);
        Task<ResponseModel> DeleteUserAsync(string userId);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<ResponseModel> AssignRolesToUserAsync(string userId, List<string> roleIds);
    }
}
