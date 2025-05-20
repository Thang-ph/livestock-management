using BusinessObjects.ConfigModels;
using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using LivestockManagementSystemAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LivestockManagementSystemAPI.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/role-management")]
    [ApiController]
    [Authorize(Roles = "Giám đốc")]
    [SwaggerTag("Quản lý vai trò: tạo, cập nhật, xóa và gán quyền cho các vai trò trong hệ thống")]
    public class RoleManagementController : BaseAPIController
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<RoleManagementController> _logger;

        public RoleManagementController(IPermissionRepository permissionRepository, ILogger<RoleManagementController> logger)
        {
            _permissionRepository = permissionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả vai trò
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả vai trò hiện có trong hệ thống kèm theo mức ưu tiên.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "id": "1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p",  -> |ID của vai trò|
        ///       "name": "Giám đốc",                            -> |Tên vai trò|
        ///       "priority": 1                                  -> |Mức ưu tiên của vai trò|
        ///     },
        ///     {
        ///       "id": "2b3c4d5e-6f7g-8h9i-0j1k-2l3m4n5o6p7q",
        ///       "name": "Quản trại",
        ///       "priority": 2
        ///     },
        ///     {
        ///       "id": "3c4d5e6f-7g8h-9i0j-1k2l-3m4n5o6p7q8r",
        ///       "name": "Trợ lý",
        ///       "priority": 3
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách vai trò kèm mức ưu tiên</returns>
        /// <response code="200">Thành công, trả về danh sách vai trò</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("roles")]
        [Permission(Permissions.ViewRoles)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRoles()
        {
            _logger.LogInformation("[RoleManagementController.GetAllRoles] - Getting all roles");
            try
            {
                var roles = await _permissionRepository.GetAllRolesWithPriorityAsync();
                _logger.LogInformation("[RoleManagementController.GetAllRoles] - Successfully retrieved {Count} roles", roles.Count());
                return GetSuccess(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.GetAllRoles] - Error occurred while getting all roles");
                return GetError("Đã xảy ra lỗi khi lấy danh sách vai trò: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của vai trò theo ID
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin chi tiết của một vai trò cụ thể dựa theo ID bao gồm cả mức ưu tiên.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p",  -> |ID của vai trò|
        ///     "name": "Giám đốc",                            -> |Tên vai trò|
        ///     "priority": 1                                  -> |Mức ưu tiên của vai trò|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của vai trò cần xem thông tin</param>
        /// <returns>Thông tin chi tiết của vai trò kèm mức ưu tiên</returns>
        /// <response code="200">Thành công, trả về thông tin chi tiết vai trò</response>
        /// <response code="404">Không tìm thấy vai trò với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("roles/{id}")]
        [Permission(Permissions.ViewRoles)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoleById(string id)
        {
            _logger.LogInformation("[RoleManagementController.GetRoleById] - Getting role by ID: {RoleId}", id);
            try
            {
                // Lấy role cơ bản
                var role = await _permissionRepository.GetRoleById(id);
                if (role == null)
                {
                    _logger.LogWarning("[RoleManagementController.GetRoleById] - Role not found with ID: {RoleId}", id);
                    return GetError("Không tìm thấy vai trò");
                }

                // Lấy priority của role
                int priority = await _permissionRepository.GetRolePriorityAsync(id);

                // Tạo đối tượng RoleWithPriority để trả về
                var roleWithPriority = new RoleWithPriority
                {
                    Id = role.Id,
                    Name = role.Name,
                    Priority = priority
                };

                _logger.LogInformation("[RoleManagementController.GetRoleById] - Successfully retrieved role with ID: {RoleId} and priority: {Priority}", id, priority);
                return GetSuccess(roleWithPriority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.GetRoleById] - Error occurred while getting role with ID: {RoleId}", id);
                return GetError("Đã xảy ra lỗi khi lấy thông tin vai trò: " + ex.Message);
            }
        }

        /// <summary>
        /// Tạo vai trò mới
        /// </summary>
        /// <remarks>
        /// API này thực hiện tạo một vai trò mới với tên cung cấp.
        /// Hệ thống sẽ tự động thiết lập mức ưu tiên thấp nhất cho vai trò mới (cao nhất hiện tại + 1).
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Kế toán"  -> |Tên vai trò mới|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "4d5e6f7g-8h9i-0j1k-2l3m-4n5o6p7q8r9s",  -> |ID của vai trò vừa tạo|
        ///     "name": "Kế toán"                              -> |Tên vai trò đã tạo|
        ///   }
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "message": "Vai trò đã tồn tại"
        /// }
        /// ```
        /// </remarks>
        /// <param name="model">Thông tin vai trò cần tạo</param>
        /// <returns>Thông tin vai trò đã tạo</returns>
        /// <response code="200">Thành công, đã tạo vai trò mới</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc vai trò đã tồn tại</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("roles")]
        [Permission(Permissions.CreateRoles)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO model)
        {
            _logger.LogInformation("[RoleManagementController.CreateRole] - Creating new role with name: {RoleName}", model.Name);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[RoleManagementController.CreateRole] - Invalid model state when creating role");
                return Error("Dữ liệu không hợp lệ", ModelState);
            }

            try
            {
                var role = await _permissionRepository.CreateRole(model.Name);
                if (role == null)
                {
                    _logger.LogWarning("[RoleManagementController.CreateRole] - Failed to create role: Role already exists with name: {RoleName}", model.Name);
                    return Error("Vai trò đã tồn tại");
                }

                // Lấy priority của role vừa tạo
                var roles = await _permissionRepository.GetAllRolesWithPriorityAsync();
                var roleWithPriority = roles.FirstOrDefault(r => r.Id == role.Id);

                _logger.LogInformation("[RoleManagementController.CreateRole] - Successfully created role with ID: {RoleId} and priority: {Priority}",
                    role.Id, roleWithPriority?.Priority);

                // Trả về thông tin role kèm theo priority
                return SaveSuccess(roleWithPriority != null ? roleWithPriority : role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.CreateRole] - Error occurred while creating role with name: {RoleName}", model.Name);
                return Error("Đã xảy ra lỗi khi tạo vai trò", ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin vai trò
        /// </summary>
        /// <remarks>
        /// API này thực hiện cập nhật tên của một vai trò đã tồn tại.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Kế toán trưởng"  -> |Tên vai trò mới|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "4d5e6f7g-8h9i-0j1k-2l3m-4n5o6p7q8r9s",  -> |ID của vai trò|
        ///     "name": "Kế toán trưởng"                       -> |Tên vai trò sau khi cập nhật|
        ///   }
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "message": "Không tìm thấy vai trò hoặc tên vai trò đã tồn tại"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của vai trò cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật vai trò</param>
        /// <returns>Thông tin vai trò sau khi cập nhật</returns>
        /// <response code="200">Thành công, đã cập nhật vai trò</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy vai trò với ID cung cấp hoặc tên vai trò mới đã tồn tại</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("roles/{id}")]
        [Permission(Permissions.EditRoles)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDTO model)
        {
            _logger.LogInformation("[RoleManagementController.UpdateRole] - Updating role with ID: {RoleId}, New Name: {RoleName}", id, model.Name);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[RoleManagementController.UpdateRole] - Invalid model state when updating role: {RoleId}", id);
                return Error("Dữ liệu không hợp lệ", ModelState);
            }

            try
            {
                var role = await _permissionRepository.UpdateRole(id, model.Name);
                if (role == null)
                {
                    _logger.LogWarning("[RoleManagementController.UpdateRole] - Failed to update role with ID: {RoleId}. Role not found or name already exists", id);
                    return Error("Không tìm thấy vai trò hoặc tên vai trò đã tồn tại");
                }

                _logger.LogInformation("[RoleManagementController.UpdateRole] - Successfully updated role with ID: {RoleId}", id);
                return SaveSuccess(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.UpdateRole] - Error occurred while updating role with ID: {RoleId}", id);
                return Error("Đã xảy ra lỗi khi cập nhật vai trò", ex.Message);
            }
        }

        /// <summary>
        /// Xóa vai trò
        /// </summary>
        /// <remarks>
        /// API này thực hiện xóa một vai trò khỏi hệ thống. Vai trò sẽ bị xóa hoàn toàn và không thể khôi phục.
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": {
        ///     "message": "Xóa vai trò thành công"  -> |Thông báo xác nhận|
        ///   }
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "message": "Không tìm thấy vai trò"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của vai trò cần xóa</param>
        /// <returns>Thông báo kết quả xóa vai trò</returns>
        /// <response code="200">Thành công, đã xóa vai trò</response>
        /// <response code="404">Không tìm thấy vai trò với ID cung cấp</response>
        /// <response code="400">Lỗi do không thể xóa vai trò do có user</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpDelete("roles/{id}")]
        [Permission(Permissions.DeleteRoles)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            _logger.LogInformation("[RoleManagementController.DeleteRole] - Deleting role with ID: {RoleId}", id);
            try
            {
                var result = await _permissionRepository.DeleteRole(id);
                if (!result)
                {
                    _logger.LogWarning("[RoleManagementController.DeleteRole] - Role not found for deletion with ID: {RoleId}", id);
                    return GetError("Không tìm thấy vai trò");
                }

                _logger.LogInformation("[RoleManagementController.DeleteRole] - Successfully deleted role with ID: {RoleId}", id);
                return SaveSuccess(new { Message = "Xóa vai trò thành công" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("[RoleManagementController.DeleteRole] - Cannot delete role with ID: {RoleId}. {Message}", id, ex.Message);
                return Error("Không thể xóa vai trò", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.DeleteRole] - Error occurred while deleting role with ID: {RoleId}", id);
                return Error("Đã xảy ra lỗi khi xóa vai trò", ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả quyền trong hệ thống
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả quyền trong hệ thống, được nhóm theo module.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "Users": [                                    -> |Module quản lý người dùng|
        ///       "Permission.Users.View",                     -> |Quyền xem người dùng|
        ///       "Permission.Users.Create",                   -> |Quyền tạo người dùng|
        ///       "Permission.Users.Edit",                     -> |Quyền sửa người dùng|
        ///       "Permission.Users.Delete",                   -> |Quyền xóa người dùng|
        ///       "Permission.Users.Block"                     -> |Quyền khóa người dùng|
        ///     ],
        ///     "Roles": [                                    -> |Module quản lý vai trò|
        ///       "Permission.Roles.View",                     -> |Quyền xem vai trò|
        ///       "Permission.Roles.Create",                   -> |Quyền tạo vai trò|
        ///       "Permission.Roles.Edit",                     -> |Quyền sửa vai trò|
        ///       "Permission.Roles.Delete",                   -> |Quyền xóa vai trò|
        ///       "Permission.Roles.ManagePermissions"         -> |Quyền quản lý phân quyền|
        ///     ],
        ///     "Livestock": [                                -> |Module quản lý gia súc|
        ///       "Permission.Livestock.View",                 -> |Quyền xem gia súc|
        ///       "Permission.Livestock.Create",               -> |Quyền tạo gia súc|
        ///       "Permission.Livestock.Edit",                 -> |Quyền sửa gia súc|
        ///       "Permission.Livestock.Delete"                -> |Quyền xóa gia súc|
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách quyền được nhóm theo module</returns>
        /// <response code="200">Thành công, trả về danh sách quyền</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("permissions")]
        [Permission(Permissions.ManageRolePermissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPermissions()
        {
            _logger.LogInformation("[RoleManagementController.GetAllPermissions] - Getting all permissions");
            try
            {
                var permissions = await _permissionRepository.GetAllPermissionsByModule();
                _logger.LogInformation("[RoleManagementController.GetAllPermissions] - Successfully retrieved permissions by module");
                return GetSuccess(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.GetAllPermissions] - Error occurred while getting all permissions");
                return GetError("Đã xảy ra lỗi khi lấy danh sách quyền: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả quyền trong hệ thống bằng tiếng Việt
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả quyền trong hệ thống bằng tiếng Việt, được nhóm theo module.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "Người dùng": {                                      
        ///       "Permission.Users.View": "Xem người dùng",                     
        ///       "Permission.Users.Create": "Thêm người dùng",                   
        ///       "Permission.Users.Edit": "Chỉnh sửa người dùng",                     
        ///       "Permission.Users.Delete": "Xoá người dùng",                   
        ///       "Permission.Users.Block": "Khóa người dùng"                     
        ///     },
        ///     "Vai trò": {                                    
        ///       "Permission.Roles.View": "Xem vai trò",                     
        ///       "Permission.Roles.Create": "Thêm vai trò",                   
        ///       "Permission.Roles.Edit": "Chỉnh sửa vai trò",                     
        ///       "Permission.Roles.Delete": "Xoá vai trò",                   
        ///       "Permission.Roles.ManagePermissions": "Quản lý phân quyền"         
        ///     },
        ///     "Vật nuôi": {                               
        ///       "Permission.Livestock.View": "Xem vật nuôi",                 
        ///       "Permission.Livestock.Create": "Thêm vật nuôi",               
        ///       "Permission.Livestock.Edit": "Chỉnh sửa vật nuôi",                 
        ///       "Permission.Livestock.Delete": "Xoá vật nuôi"                
        ///     }
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách quyền bằng tiếng Việt được nhóm theo module</returns>
        /// <response code="200">Thành công, trả về danh sách quyền bằng tiếng Việt</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("permissions/vietnamese")]
        [Permission(Permissions.ManageRolePermissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllVietnamesePermissions()
        {
            _logger.LogInformation("[RoleManagementController.GetAllVietnamesePermissions] - Getting all Vietnamese permissions");
            try
            {
                var permissions = await _permissionRepository.GetVietnamesePermissionsByModule();
                _logger.LogInformation("[RoleManagementController.GetAllVietnamesePermissions] - Successfully retrieved Vietnamese permissions by module");
                return GetSuccess(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.GetAllVietnamesePermissions] - Error occurred while getting all Vietnamese permissions");
                return GetError("Đã xảy ra lỗi khi lấy danh sách quyền tiếng Việt: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách quyền của vai trò
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các quyền đã được gán cho một vai trò cụ thể.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "roleId": "1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p",  -> |ID của vai trò|
        ///     "roleName": "Nhân viên kỹ thuật",                  -> |Tên vai trò|
        ///     "permissions": [                                   -> |Danh sách quyền được gán|
        ///       "Permission.Livestock.View",
        ///       "Permission.Medical.View",
        ///       "Permission.Medical.Create",
        ///       "Permission.Medical.Edit"
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="roleId">ID của vai trò cần xem quyền</param>
        /// <returns>Danh sách quyền của vai trò</returns>
        /// <response code="200">Thành công, trả về danh sách quyền của vai trò</response>
        /// <response code="404">Không tìm thấy vai trò với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("roles/{roleId}/permissions")]
        [Permission(Permissions.ManageRolePermissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRolePermissions(string roleId)
        {
            _logger.LogInformation("[RoleManagementController.GetRolePermissions] - Getting permissions for role ID: {RoleId}", roleId);
            try
            {
                var role = await _permissionRepository.GetRoleById(roleId);
                if (role == null)
                {
                    _logger.LogWarning("[RoleManagementController.GetRolePermissions] - Role not found with ID: {RoleId}", roleId);
                    return GetError("Không tìm thấy vai trò");
                }

                var permissions = await _permissionRepository.GetPermissionsForRole(roleId);
                _logger.LogInformation("[RoleManagementController.GetRolePermissions] - Successfully retrieved {Count} permissions for role ID: {RoleId}", permissions.Count(), roleId);

                // Thêm tên hiển thị tiếng Việt cho mỗi quyền
                var vietnamesePermissions = permissions.ToDictionary(
                    p => p,
                    p => Permissions.GetVietnamesePermissionName(p)
                );

                return GetSuccess(new RolePermissionsDTO
                {
                    RoleId = roleId,
                    RoleName = role.Name,
                    Permissions = permissions,
                    VietnamesePermissions = vietnamesePermissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.GetRolePermissions] - Error occurred while getting permissions for role with ID: {RoleId}", roleId);
                return GetError("Đã xảy ra lỗi khi lấy quyền của vai trò: " + ex.Message);
            }
        }

        /// <summary>
        /// Thiết lập quyền cho vai trò
        /// </summary>
        /// <remarks>
        /// API này thực hiện gán quyền cho một vai trò. 
        /// Danh sách quyền cung cấp sẽ thay thế hoàn toàn danh sách quyền hiện tại của vai trò.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// [
        ///   "Permission.Livestock.View",        -> |Quyền xem gia súc|
        ///   "Permission.Medical.View",          -> |Quyền xem y tế|
        ///   "Permission.Medical.Create",        -> |Quyền tạo y tế|
        ///   "Permission.Medical.Edit"           -> |Quyền sửa y tế|
        /// ]
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": [
        ///     "Permission.Livestock.View",
        ///     "Permission.Medical.View",
        ///     "Permission.Medical.Create",
        ///     "Permission.Medical.Edit"
        ///   ]
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "message": "Không tìm thấy vai trò"
        /// }
        /// ```
        /// </remarks>
        /// <param name="roleId">ID của vai trò cần thiết lập quyền</param>
        /// <param name="permissions">Danh sách quyền cần gán</param>
        /// <returns>Danh sách quyền đã được gán</returns>
        /// <response code="200">Thành công, đã thiết lập quyền cho vai trò</response>
        /// <response code="400">Không thể cập nhật quyền</response>
        /// <response code="404">Không tìm thấy vai trò với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("roles/{roleId}/permissions")]
        [Permission(Permissions.ManageRolePermissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetRolePermissions(string roleId, [FromBody] List<string> permissions)
        {
            _logger.LogInformation("[RoleManagementController.SetRolePermissions] - Setting permissions for role ID: {RoleId}, Permissions Count: {Count}", roleId, permissions.Count);
            try
            {
                var role = await _permissionRepository.GetRoleById(roleId);
                if (role == null)
                {
                    _logger.LogWarning("[RoleManagementController.SetRolePermissions] - Role not found with ID: {RoleId}", roleId);
                    return GetError("Không tìm thấy vai trò");
                }

                var result = await _permissionRepository.SetPermissionsForRole(roleId, permissions);
                if (!result)
                {
                    _logger.LogWarning("[RoleManagementController.SetRolePermissions] - Failed to update permissions for role ID: {RoleId}", roleId);
                    return Error("Không thể cập nhật quyền cho vai trò");
                }

                _logger.LogInformation("[RoleManagementController.SetRolePermissions] - Successfully updated permissions for role ID: {RoleId}", roleId);
                return SaveSuccess(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.SetRolePermissions] - Error occurred while updating permissions for role with ID: {RoleId}", roleId);
                return Error("Đã xảy ra lỗi khi cập nhật quyền cho vai trò", ex.Message);
            }
        }

        /// <summary>
        /// Thiết lập mức ưu tiên cho vai trò
        /// </summary>
        /// <remarks>
        /// Thiết lập mức ưu tiên cho vai trò, mức ưu tiên thấp nhất là 1 (cao nhất).
        /// Khi một người dùng có nhiều vai trò, vai trò có mức ưu tiên cao nhất (số nhỏ nhất) sẽ được coi là vai trò chính.
        /// 
        /// Ví dụ Request Body (gửi số nguyên trực tiếp):
        /// ```json
        /// 2
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "message": "Đã thiết lập mức ưu tiên cho vai trò thành công"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "message": "Không tìm thấy vai trò"
        /// }
        /// ```
        /// </remarks>
        /// <param name="roleId">ID của vai trò</param>
        /// <param name="priority">Mức ưu tiên (1 là cao nhất)</param>
        /// <returns>Kết quả thiết lập mức ưu tiên</returns>
        /// <response code="200">Thành công, đã thiết lập mức ưu tiên</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("roles/{roleId}/priority")]
        [Permission(Permissions.EditRoles)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetRolePriority(string roleId, [FromBody] int priority)
        {
            _logger.LogInformation("[RoleManagementController.SetRolePriority] - Setting priority {Priority} for role {RoleId}",
                priority, roleId);

            try
            {
                var result = await _permissionRepository.SetRolePriorityAsync(roleId, priority);
                if (result.IsSuccess)
                {
                    return Success(null, result.Message);
                }
                return Error(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.SetRolePriority] - Error setting priority for role");
                return Error("Đã xảy ra lỗi khi thiết lập mức ưu tiên cho vai trò");
            }
        }

        /// <summary>
        /// Lấy danh sách vai trò kèm mức ưu tiên
        /// </summary>
        /// <remarks>
        /// Lấy danh sách tất cả vai trò trong hệ thống kèm theo mức ưu tiên của chúng.
        /// Mức ưu tiên thấp nhất là 1 (cao nhất), các số lớn hơn thể hiện mức ưu tiên thấp hơn.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "id": "1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p",
        ///       "name": "Giám đốc",
        ///       "priority": 1
        ///     },
        ///     {
        ///       "id": "2b3c4d5e-6f7g-8h9i-0j1k-2l3m4n5o6p7q",
        ///       "name": "Quản trại",
        ///       "priority": 2
        ///     }
        ///   ],
        ///   "message": "Lấy danh sách vai trò và mức ưu tiên thành công"
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách vai trò và mức ưu tiên</returns>
        /// <response code="200">Thành công, trả về danh sách vai trò và mức ưu tiên</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("roles/with-priority")]
        [Permission(Permissions.ViewRoles)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRolesWithPriority()
        {
            _logger.LogInformation("[RoleManagementController.GetRolesWithPriority] - Getting all roles with priority");

            try
            {
                var roles = await _permissionRepository.GetAllRolesWithPriorityAsync();
                return Success(roles, "Lấy danh sách vai trò và mức ưu tiên thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RoleManagementController.GetRolesWithPriority] - Error getting roles with priority");
                return Error("Đã xảy ra lỗi khi lấy danh sách vai trò và mức ưu tiên");
            }
        }
    }
}
