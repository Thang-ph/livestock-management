using AutoMapper;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Utils;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repository.Services
{
    public class UserManagementService : IUserManagementRepository
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly LmsContext _context;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            LmsContext context,
            ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {

            var users = await _userManager.Users.ToListAsync();
            var userDTOs = new List<UserDTO>();

            foreach (var user in users)
            {
                var userDTO = await CreateUserDTOFromIdentityUser(user);
                // Loại bỏ người dùng có vai trò Giám đốc
                if (!userDTO.Roles.Contains("Giám đốc"))
                {
                    userDTOs.Add(userDTO);
                }
            }

            _logger.LogInformation("[UserManagementService.GetAllUsersAsync] - Successfully retrieved {UserCount} users after filtering", userDTOs.Count);
            return userDTOs;
        }

        public async Task<List<UserDTO>> SearchUsersAsync(string searchTerm, List<string> roles)
        {
            _logger.LogInformation("[UserManagementService.SearchUsersAsync] - Starting to search users with term: {SearchTerm}, roles: {Roles}",
                searchTerm, roles != null ? string.Join(", ", roles) : "none");

            // Lấy tất cả người dùng
            var users = await _userManager.Users.ToListAsync();
            var userDTOs = new List<UserDTO>();

            _logger.LogDebug("[UserManagementService.SearchUsersAsync] - Found {UserCount} users total before filtering", users.Count);

            // Lọc người dùng theo điều kiện tìm kiếm
            foreach (var user in users)
            {
                // Chuyển đổi user thành DTO để có thông tin vai trò
                var userDTO = await CreateUserDTOFromIdentityUser(user);

                // Loại bỏ người dùng có vai trò Giám đốc
                if (userDTO.Roles.Contains("Giám đốc"))
                {
                    continue;
                }

                // Lọc theo vai trò nếu có
                if (roles != null && roles.Count > 0)
                {
                    bool hasAnyRole = userDTO.Roles.Any(r => roles.Contains(r));
                    if (!hasAnyRole)
                        continue;
                }

                // Lọc theo email/số điện thoại nếu có
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    bool matchesSearch = (user.Email?.ToLower().Contains(searchTerm) == true) ||
                                         (user.PhoneNumber?.Contains(searchTerm) == true) ||
                                         (user.UserName?.ToLower().Contains(searchTerm) == true);

                    if (!matchesSearch)
                        continue;
                }

                // Nếu thỏa mãn tất cả điều kiện thì thêm vào kết quả
                userDTOs.Add(userDTO);
            }

            _logger.LogInformation("[UserManagementService.SearchUsersAsync] - Successfully found {UserCount} users after filtering", userDTOs.Count);
            return userDTOs;
        }

        public async Task<UserDTO> GetUserByIdAsync(string userId)
        {
            _logger.LogInformation("[UserManagementService.GetUserByIdAsync] - Starting to fetch user with ID: {UserId}", userId);

            var user = await FindUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var userDTO = await CreateUserDTOFromIdentityUser(user);

            _logger.LogInformation("[UserManagementService.GetUserByIdAsync] - Successfully retrieved user with ID: {UserId}", userId);
            return userDTO;
        }

        public async Task<ResponseModel> CreateUserAsync(CreateUserDTO model)
        {
            _logger.LogInformation("[UserManagementService.CreateUserAsync] - Starting to create user with email: {Email}", model.Email);

            // Tiến hành kiểm tra tính hợp lệ của dữ liệu
            var validationErrors = await ValidateUserCreationData(model);
            if (validationErrors.Any())
            {
                return CreateErrorResponse(validationErrors);
            }

            // Tạo người dùng mới
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            // Sử dụng mật khẩu mặc định nếu không có mật khẩu được cung cấp
            string password = string.IsNullOrEmpty(model.Password) ? "luavang@2010" : model.Password;

            _logger.LogDebug("[UserManagementService.CreateUserAsync] - Attempting to create user {Username} with {PasswordType} password",
                user.UserName, string.IsNullOrEmpty(model.Password) ? "default" : "provided");

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = IdentityErrorTranslator.TranslateErrors(result.Errors);
                _logger.LogError("[UserManagementService.CreateUserAsync] - Failed to create user: {Errors}", errors);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = errors
                };
            }

            // Gán vai trò cho người dùng
            await AssignRolesToUser(user, model.Roles);

            _logger.LogInformation("[UserManagementService.CreateUserAsync] - Successfully created user with ID: {UserId}", user.Id);
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Tạo tài khoản thành công",
                Data = await CreateUserDTOFromIdentityUser(user)
            };
        }

        public async Task<ResponseModel> UpdateUserAsync(string userId, UpdateUserDTO model)
        {
            _logger.LogInformation("[UserManagementService.UpdateUserAsync] - Starting to update user with ID: {UserId}", userId);

            // Tìm người dùng theo ID
            var user = await FindUserByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người dùng"
                };
            }

            // Kiểm tra tính hợp lệ của dữ liệu cập nhật
            var validationErrors = await ValidateUserUpdateData(userId, model);
            if (validationErrors.Any())
            {
                return CreateErrorResponse(validationErrors);
            }

            // Chỉ cập nhật mật khẩu khi có mật khẩu được cung cấp
            if (!string.IsNullOrEmpty(model.Password))
            {
                // Đảm bảo mật khẩu và xác nhận mật khẩu khớp nhau
                if (string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Vui lòng xác nhận mật khẩu"
                    };
                }

                if (model.Password != model.ConfirmPassword)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Mật khẩu và xác nhận mật khẩu không khớp"
                    };
                }

                // Cập nhật mật khẩu
                _logger.LogDebug("[UserManagementService.UpdateUserAsync] - Updating password for user {UserId}", userId);
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!passwordResult.Succeeded)
                {
                    var errors = IdentityErrorTranslator.TranslateErrors(passwordResult.Errors);
                    _logger.LogError("[UserManagementService.UpdateUserAsync] - Failed to update password: {Errors}", errors);
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Lỗi khi cập nhật mật khẩu: " + errors
                    };
                }
            }

            // Cập nhật thông tin người dùng
            UpdateUserProperties(user, model);

            // Lưu thay đổi
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = IdentityErrorTranslator.TranslateErrors(result.Errors);
                _logger.LogError("[UserManagementService.UpdateUserAsync] - Failed to update user: {Errors}", errors);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = errors
                };
            }

            // Cập nhật vai trò nếu cần
            if (model.Roles != null && model.Roles.Any())
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var roleUpdateSuccess = await UpdateUserRoles(user, currentRoles, model.Roles);
                if (!roleUpdateSuccess)
                {
                    _logger.LogWarning("[UserManagementService.UpdateUserAsync] - User info updated but role assignment failed for user {UserId}", userId);
                    return new ResponseModel
                    {
                        IsSuccess = true,
                        Message = "Cập nhật thông tin người dùng thành công nhưng phân quyền không thành công",
                        Data = await CreateUserDTOFromIdentityUser(user)
                    };
                }
            }

            _logger.LogInformation("[UserManagementService.UpdateUserAsync] - Successfully updated user with ID: {UserId}", userId);
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Cập nhật thông tin người dùng thành công",
                Data = await CreateUserDTOFromIdentityUser(user)
            };
        }

        public async Task<ResponseModel> BlockUserAsync(string userId, bool isBlocked)
        {
            _logger.LogInformation("[UserManagementService.BlockUserAsync] - Starting to {Action} user with ID: {UserId}",
                isBlocked ? "block" : "unblock", userId);

            var user = await FindUserByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người dùng"
                };
            }

            // Cập nhật trạng thái khóa
            if (isBlocked)
            {
                _logger.LogDebug("[UserManagementService.BlockUserAsync] - Locking user {UserId}", userId);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                _logger.LogDebug("[UserManagementService.BlockUserAsync] - Unlocking user {UserId}", userId);
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            _logger.LogInformation("[UserManagementService.BlockUserAsync] - Successfully {Action} user with ID: {UserId}",
                isBlocked ? "blocked" : "unblocked", userId);

            return new ResponseModel
            {
                IsSuccess = true,
                Message = isBlocked ? "Đã khóa tài khoản" : "Đã mở khóa tài khoản",
                Data = await CreateUserDTOFromIdentityUser(user)
            };
        }

        public async Task<ResponseModel> DeleteUserAsync(string userId)
        {
            _logger.LogInformation("[UserManagementService.DeleteUserAsync] - Starting to delete user with ID: {UserId}", userId);

            var user = await FindUserByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người dùng"
                };
            }

            _logger.LogDebug("[UserManagementService.DeleteUserAsync] - Attempting to delete user {UserId}", userId);
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = IdentityErrorTranslator.TranslateErrors(result.Errors);
                _logger.LogError("[UserManagementService.DeleteUserAsync] - Failed to delete user: {Errors}", errors);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = errors
                };
            }

            _logger.LogInformation("[UserManagementService.DeleteUserAsync] - Successfully deleted user with ID: {UserId}", userId);
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Xóa tài khoản thành công"
            };
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            _logger.LogInformation("[UserManagementService.GetUserRolesAsync] - Starting to fetch roles for user with ID: {UserId}", userId);

            var user = await FindUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[UserManagementService.GetUserRolesAsync] - User with ID {UserId} not found", userId);
                return new List<string>();
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            _logger.LogInformation("[UserManagementService.GetUserRolesAsync] - Successfully retrieved {RoleCount} roles for user {UserId}",
                roles.Count, userId);

            return roles;
        }

        public async Task<ResponseModel> AssignRolesToUserAsync(string userId, List<string> roleNames)
        {
            _logger.LogInformation("[UserManagementService.AssignRolesToUserAsync] - Starting to assign roles to user with ID: {UserId}. Roles: {Roles}",
                userId, roleNames != null ? string.Join(", ", roleNames) : "none");

            var user = await FindUserByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người dùng"
                };
            }

            // Lấy vai trò hiện tại
            var currentRoles = await _userManager.GetRolesAsync(user);
            _logger.LogDebug("[UserManagementService.AssignRolesToUserAsync] - User {UserId} current roles: {Roles}",
                userId, string.Join(", ", currentRoles));

            // Cập nhật vai trò
            var success = await UpdateUserRoles(user, currentRoles, roleNames);
            if (!success)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Phân quyền cho người dùng không thành công"
                };
            }

            var finalRoles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("[UserManagementService.AssignRolesToUserAsync] - Successfully assigned roles to user {UserId}. Final roles: {Roles}",
                userId, string.Join(", ", finalRoles));

            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Phân quyền cho người dùng thành công",
                Data = finalRoles
            };
        }

        #region Private Methods

        private async Task<IdentityUser> FindUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[UserManagementService] - User with ID {UserId} not found", userId);
            }
            return user;
        }

        private async Task<UserDTO> CreateUserDTOFromIdentityUser(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var rolesWithPriority = new List<RoleWithPriority>();
            string primaryRole = null;
            int highestPriority = int.MaxValue;

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var claims = await _roleManager.GetClaimsAsync(role);
                var priorityClaim = claims.FirstOrDefault(c => c.Type == "Priority");

                int priority = int.MaxValue;
                if (priorityClaim != null && int.TryParse(priorityClaim.Value, out int parsedPriority))
                {
                    priority = parsedPriority;
                }

                rolesWithPriority.Add(new RoleWithPriority
                {
                    Id = role.Id,
                    Name = roleName,
                    Priority = priority
                });

                if (priority < highestPriority)
                {
                    highestPriority = priority;
                    primaryRole = roleName;
                }
            }

            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsLocked = await _userManager.IsLockedOutAsync(user),
                Roles = roles,
                PrimaryRole = primaryRole,
                RolesWithPriority = rolesWithPriority
            };
        }

        private async Task<List<string>> ValidateUserCreationData(CreateUserDTO model)
        {
            List<string> validationErrors = new List<string>();

            // Kiểm tra email đã tồn tại chưa
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                _logger.LogWarning("[UserManagementService] - Email {Email} already in use", model.Email);
                validationErrors.Add("Email đã được sử dụng");
            }

            // Kiểm tra số điện thoại đã tồn tại chưa
            if (!string.IsNullOrEmpty(model.PhoneNumber) &&
                await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber))
            {
                _logger.LogWarning("[UserManagementService] - Phone number {Phone} already in use", model.PhoneNumber);
                validationErrors.Add("Số điện thoại đã được sử dụng");
            }

            return validationErrors;
        }

        private async Task<List<string>> ValidateUserUpdateData(string userId, UpdateUserDTO model)
        {
            List<string> validationErrors = new List<string>();

            // Kiểm tra email đã tồn tại chưa
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    _logger.LogWarning("[UserManagementService] - Email {Email} already in use", model.Email);
                    validationErrors.Add("Email đã được sử dụng");
                }
            }

            // Kiểm tra số điện thoại đã tồn tại chưa
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != userId);
                if (existingUser != null)
                {
                    _logger.LogWarning("[UserManagementService] - Phone number {Phone} already in use", model.PhoneNumber);
                    validationErrors.Add("Số điện thoại đã được sử dụng");
                }
            }

            return validationErrors;
        }

        private void UpdateUserProperties(IdentityUser user, UpdateUserDTO model)
        {
            _logger.LogDebug("[UserManagementService] - Updating user properties for user {UserId}", user.Id);

            if (!string.IsNullOrEmpty(model.UserName) && user.UserName != model.UserName)
            {
                _logger.LogDebug("[UserManagementService] - Updating username from {OldUsername} to {NewUsername}",
                    user.UserName, model.UserName);
                user.UserName = model.UserName;
            }

            if (!string.IsNullOrEmpty(model.Email) && user.Email != model.Email)
            {
                _logger.LogDebug("[UserManagementService] - Updating email from {OldEmail} to {NewEmail}",
                    user.Email, model.Email);
                user.Email = model.Email;
                user.EmailConfirmed = true;
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber) && user.PhoneNumber != model.PhoneNumber)
            {
                _logger.LogDebug("[UserManagementService] - Updating phone from {OldPhone} to {NewPhone}",
                    user.PhoneNumber, model.PhoneNumber);
                user.PhoneNumber = model.PhoneNumber;
            }
        }

        private ResponseModel CreateErrorResponse(List<string> errors)
        {
            var errorMessage = string.Join(", ", errors);
            _logger.LogWarning("[UserManagementService] - Validation failed: {Errors}", errorMessage);
            return new ResponseModel
            {
                IsSuccess = false,
                Message = errorMessage
            };
        }

        private async Task AssignRolesToUser(IdentityUser user, List<string> roleNames)
        {
            if (roleNames == null || !roleNames.Any())
                return;

            _logger.LogDebug("[UserManagementService] - Assigning roles to user {UserId}: {Roles}",
                user.Id, string.Join(", ", roleNames));

            foreach (var role in roleNames)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    _logger.LogWarning("[UserManagementService] - Role {Role} does not exist", role);
                }
            }
        }

        private async Task<bool> UpdateUserRoles(IdentityUser user, IList<string> currentRoles, List<string> newRoles)
        {
            if (newRoles == null || !newRoles.Any())
                return true;

            var validRoles = new List<string>();
            foreach (var roleName in newRoles)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    validRoles.Add(roleName);
                }
                else
                {
                    _logger.LogWarning("[UserManagementService] - Role {Role} does not exist", roleName);
                }
            }

            if (!validRoles.Any())
            {
                _logger.LogWarning("[UserManagementService] - No valid roles found to assign");
                return false;
            }

            // Xóa tất cả vai trò hiện tại
            if (currentRoles.Any())
            {
                _logger.LogDebug("[UserManagementService] - Removing current roles from user {UserId}", user.Id);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Thêm vai trò mới
            _logger.LogDebug("[UserManagementService] - Adding roles to user {UserId}: {Roles}",
                user.Id, string.Join(", ", validRoles));
            await _userManager.AddToRolesAsync(user, validRoles);

            return true;
        }

        #endregion
    }
}
