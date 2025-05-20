using BusinessObjects.ConfigModels;
using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using LivestockManagementSystemAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/user-management")]
    [ApiController]
    [Authorize(Roles = "Giám đốc")]
    [SwaggerTag("Quản lý người dùng: quản lý tài khoản và phân quyền người dùng trong hệ thống")]
    public class UserManagementController : BaseAPIController
    {
        private readonly IUserManagementRepository _userManagementRepository;
        private readonly ILogger<UserManagementController> _logger;
        private readonly IPermissionRepository _permissionRepository;

        public UserManagementController(
            IUserManagementRepository userManagementRepository,
            ILogger<UserManagementController> logger,
            IPermissionRepository permissionRepository)
        {
            _userManagementRepository = userManagementRepository;
            _logger = logger;
            _permissionRepository = permissionRepository;
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả người dùng trong hệ thống kèm theo thông tin cơ bản và vai trò của họ.
        /// Có thể tìm kiếm người dùng theo email/số điện thoại bằng tham số searchTerm và lọc theo vai trò bằng tham số roles.
        /// 
        /// Ví dụ Request:
        /// GET /api/user-management/users?searchTerm=example@gmail.com và roles=Nhân viên kỹ thuật,Quản lý kho
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "id": "5f8d3b7e-d8f9-4e7c-9d7a-b8c3d4e5f6g7",  -> |ID duy nhất của người dùng|
        ///       "userName": "nguyenvana",                      -> |Tên đăng nhập|
        ///       "email": "nguyenvana@example.com",             -> |Địa chỉ email|
        ///       "phoneNumber": "0912345678",                   -> |Số điện thoại|
        ///       "isLocked": false,                             -> |Trạng thái khóa tài khoản|
        ///       "roles": [                                     -> |Danh sách vai trò|
        ///         "Nhân viên kỹ thuật",
        ///         "Quản lý kho"
        ///       ]
        ///     },
        ///     {
        ///       "id": "2a1b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p",
        ///       "userName": "tranvanb",
        ///       "email": "tranvanb@example.com",
        ///       "phoneNumber": "0978123456",
        ///       "isLocked": true,
        ///       "roles": [
        ///         "Kế toán"
        ///       ]
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="searchTerm">Từ khóa tìm kiếm theo email, số điện thoại hoặc tên đăng nhập (tùy chọn)</param>
        /// <param name="roles">Danh sách vai trò để lọc, phân cách bởi dấu phẩy (tùy chọn)</param>
        /// <returns>Danh sách người dùng</returns>
        /// <response code="200">Thành công, trả về danh sách người dùng</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("users")]
        [Permission(Permissions.ViewUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers([FromQuery] string searchTerm = null, [FromQuery] string roles = null)
        {
            _logger.LogInformation("[UserManagementController.GetAllUsers] - Request received with search term: {SearchTerm}, roles: {Roles}",
                searchTerm, roles);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                List<UserDTO> users;
                List<string> rolesList = null;

                // Chuyển đổi chuỗi roles thành danh sách nếu có
                if (!string.IsNullOrEmpty(roles))
                {
                    rolesList = roles.Split(',').Select(r => r.Trim()).ToList();
                }

                // Nếu có tìm kiếm hoặc lọc thì gọi phương thức tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm) || (rolesList != null && rolesList.Count > 0))
                {
                    users = await _userManagementRepository.SearchUsersAsync(searchTerm, rolesList);
                }
                else
                {
                    users = await _userManagementRepository.GetAllUsersAsync();
                }

                stopwatch.Stop();
                _logger.LogInformation("[UserManagementController.GetAllUsers] - Successfully retrieved {UserCount} users", users.Count());
                return GetSuccess(users);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[UserManagementController.GetAllUsers] - Error retrieving users");
                return GetError("Đã xảy ra lỗi khi lấy danh sách người dùng: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của người dùng theo ID
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin chi tiết của một người dùng cụ thể dựa theo ID.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "5f8d3b7e-d8f9-4e7c-9d7a-b8c3d4e5f6g7",  -> |ID duy nhất của người dùng|
        ///     "userName": "nguyenvana",                      -> |Tên đăng nhập|
        ///     "email": "nguyenvana@example.com",             -> |Địa chỉ email|
        ///     "phoneNumber": "0912345678",                   -> |Số điện thoại|
        ///     "isLocked": false,                             -> |Trạng thái khóa tài khoản|
        ///     "roles": [                                     -> |Danh sách vai trò|
        ///       "Nhân viên kỹ thuật",
        ///       "Quản lý kho"
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của người dùng cần xem thông tin</param>
        /// <returns>Thông tin chi tiết của người dùng</returns>
        /// <response code="200">Thành công, trả về thông tin người dùng</response>
        /// <response code="404">Không tìm thấy người dùng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("users/{id}")]
        [Permission(Permissions.ViewUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation("[UserManagementController.GetUserById] - Request received for user ID: {UserId}", id);
            try
            {
                var user = await _userManagementRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("[UserManagementController.GetUserById] - User not found with ID: {UserId}", id);
                    return GetError("Không tìm thấy người dùng");
                }

                _logger.LogInformation("[UserManagementController.GetUserById] - Successfully retrieved user with ID: {UserId}", id);
                return GetSuccess(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserManagementController.GetUserById] - Error occurred while getting user with ID: {UserId}", id);
                return GetError("Đã xảy ra lỗi khi lấy thông tin người dùng: " + ex.Message);
            }
        }

        /// <summary>
        /// Tạo người dùng mới
        /// </summary>
        /// <remarks>
        /// API này thực hiện tạo một người dùng mới với thông tin cung cấp.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "userName": "nguyenvana",                  -> |Tên đăng nhập|
        ///   "email": "nguyenvana@example.com",         -> |Địa chỉ email|
        ///   "phoneNumber": "0912345678",               -> |Số điện thoại|
        ///   "password": "P@ssw0rd123",                 -> |Mật khẩu|
        ///   "roles": [                                 -> |Danh sách vai trò|
        ///     "Nhân viên kỹ thuật",
        ///     "Quản lý kho"
        ///   ]
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": "5f8d3b7e-d8f9-4e7c-9d7a-b8c3d4e5f6g7" -> |ID của người dùng vừa tạo|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "message": "Email đã được sử dụng"
        /// }
        /// ```
        /// </remarks>
        /// <param name="model">Thông tin người dùng cần tạo</param>
        /// <returns>ID của người dùng vừa tạo</returns>
        /// <response code="200">Thành công, đã tạo người dùng</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc email/số điện thoại đã tồn tại</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("users")]
        [Permission(Permissions.CreateUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO model)
        {
            _logger.LogInformation("[UserManagementController.CreateUser] - Create user request received for: {Email}", model.Email);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                _logger.LogWarning("[UserManagementController.CreateUser] - Validation failed: {Errors}", string.Join(" | ", errors));
                return Error(string.Join(" | ", errors));
            }

            try
            {
                var result = await _userManagementRepository.CreateUserAsync(model);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[UserManagementController.CreateUser] - User created successfully: {UserId}", result.Data);
                    return SaveSuccess(result.Data);
                }

                _logger.LogWarning("[UserManagementController.CreateUser] - User creation failed: {Message}", result.Message);
                return Error(result.Message, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserManagementController.CreateUser] - Error creating user for email: {Email}", model.Email);
                return Error("Đã xảy ra lỗi khi tạo người dùng", ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        /// <remarks>
        /// API này thực hiện cập nhật thông tin của một người dùng đã tồn tại.
        /// Các trường dữ liệu có thể cập nhật bao gồm: thông tin cơ bản, vai trò và mật khẩu (tùy chọn).
        /// Nếu không cung cấp mật khẩu, người dùng vẫn giữ mật khẩu hiện tại.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "userName": "nguyenvana_updated",          -> |Tên đăng nhập mới|
        ///   "email": "nguyenvana_new@example.com",     -> |Địa chỉ email mới|
        ///   "phoneNumber": "0987654321",               -> |Số điện thoại mới|
        ///   "roles": ["Quản trại", "Trợ lý"],          -> |Vai trò mới (tùy chọn)|
        ///   "password": "NewPassword123",              -> |Mật khẩu mới (tùy chọn, có thể bỏ trống)|
        ///   "confirmPassword": "NewPassword123"        -> |Xác nhận mật khẩu (bắt buộc nếu có password)|
        /// }
        /// ```
        /// 
        /// Ví dụ Request Body (không cập nhật mật khẩu):
        /// ```json
        /// {
        ///   "userName": "nguyenvana_updated",
        ///   "email": "nguyenvana_new@example.com",
        ///   "phoneNumber": "0987654321",
        ///   "roles": ["Quản trại", "Trợ lý"]
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "5f8d3b7e-d8f9-4e7c-9d7a-b8c3d4e5f6g7",
        ///     "userName": "nguyenvana_updated",
        ///     "email": "nguyenvana_new@example.com",
        ///     "phoneNumber": "0987654321",
        ///     "isLocked": false,
        ///     "roles": [
        ///       "Quản trại",
        ///       "Trợ lý"
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của người dùng cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật</param>
        /// <returns>Thông tin người dùng sau khi cập nhật</returns>
        /// <response code="200">Thành công, đã cập nhật người dùng</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy người dùng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("users/{id}")]
        [Permission(Permissions.EditUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDTO model)
        {
            _logger.LogInformation("[UserManagementController.UpdateUser] - Update user request received for user ID: {UserId}", id);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                _logger.LogWarning("[UserManagementController.UpdateUser] - Validation failed: {Errors}", string.Join(" | ", errors));
                return Error(string.Join(" | ", errors));
            }

            // Chỉ kiểm tra mật khẩu khi có mật khẩu được cung cấp
            if (!string.IsNullOrEmpty(model.Password))
            {
                // Kiểm tra nếu có mật khẩu thì phải có xác nhận mật khẩu
                if (string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    _logger.LogWarning("[UserManagementController.UpdateUser] - Password provided but confirmation missing");
                    return Error("Vui lòng xác nhận mật khẩu");
                }

                // Kiểm tra nếu mật khẩu và xác nhận mật khẩu không khớp
                if (model.Password != model.ConfirmPassword)
                {
                    _logger.LogWarning("[UserManagementController.UpdateUser] - Password and confirmation do not match");
                    return Error("Mật khẩu và xác nhận mật khẩu không khớp");
                }
            }

            try
            {
                var result = await _userManagementRepository.UpdateUserAsync(id, model);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[UserManagementController.UpdateUser] - User updated successfully: {UserId}", id);
                    return SaveSuccess(result.Data);
                }

                if (result.Message == "Không tìm thấy người dùng")
                {
                    _logger.LogWarning("[UserManagementController.UpdateUser] - User not found for update with ID: {UserId}", id);
                    return GetError(result.Message);
                }

                _logger.LogWarning("[UserManagementController.UpdateUser] - User update failed: {UserId}, Message: {Message}", id, result.Message);
                return Error(result.Message, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserManagementController.UpdateUser] - Error occurred while updating user with ID: {UserId}", id);
                return Error("Đã xảy ra lỗi khi cập nhật người dùng", ex.Message);
            }
        }

        /// <summary>
        /// Khóa hoặc mở khóa tài khoản người dùng
        /// </summary>
        /// <remarks>
        /// API này thực hiện khóa hoặc mở khóa tài khoản của một người dùng.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// true  -> |true: khóa tài khoản, false: mở khóa tài khoản|
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "5f8d3b7e-d8f9-4e7c-9d7a-b8c3d4e5f6g7",
        ///     "userName": "nguyenvana",
        ///     "email": "nguyenvana@example.com",
        ///     "phoneNumber": "0912345678",
        ///     "isLocked": true,
        ///     "roles": [
        ///       "Nhân viên kỹ thuật",
        ///       "Quản lý kho"
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của người dùng cần khóa/mở khóa</param>
        /// <param name="isBlocked">true: khóa tài khoản, false: mở khóa tài khoản</param>
        /// <returns>Thông tin người dùng sau khi thay đổi trạng thái khóa</returns>
        /// <response code="200">Thành công, đã thay đổi trạng thái khóa tài khoản</response>
        /// <response code="404">Không tìm thấy người dùng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("users/{id}/block")]
        [Permission(Permissions.BlockUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BlockUser(string id, [FromBody] bool isBlocked)
        {
            _logger.LogInformation("[UserManagementController.BlockUser] - Change block status request received for user ID: {UserId}, IsBlocked: {IsBlocked}", id, isBlocked);
            try
            {
                var result = await _userManagementRepository.BlockUserAsync(id, isBlocked);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[UserManagementController.BlockUser] - User {Action} successfully: {UserId}", isBlocked ? "blocked" : "unblocked", id);
                    return SaveSuccess(result.Data);
                }

                if (result.Message == "Không tìm thấy người dùng")
                {
                    _logger.LogWarning("[UserManagementController.BlockUser] - User not found for block/unblock with ID: {UserId}", id);
                    return GetError(result.Message);
                }

                _logger.LogWarning("[UserManagementController.BlockUser] - Change block status failed for user: {UserId}, Message: {Message}", id, result.Message);
                return Error(result.Message, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserManagementController.BlockUser] - Error occurred while changing block status for user with ID: {UserId}", id);
                return Error("Đã xảy ra lỗi khi khóa/mở khóa người dùng", ex.Message);
            }
        }

        /// <summary>
        /// Xóa người dùng
        /// </summary>
        /// <remarks>
        /// API này thực hiện xóa một người dùng khỏi hệ thống.
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": true  -> |true: xóa thành công|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "message": "Không thể xóa người dùng hệ thống"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của người dùng cần xóa</param>
        /// <returns>Kết quả xóa người dùng</returns>
        /// <response code="200">Thành công, đã xóa người dùng</response>
        /// <response code="404">Không tìm thấy người dùng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpDelete("users/{id}")]
        [Permission(Permissions.DeleteUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            _logger.LogInformation("[UserManagementController.DeleteUser] - Delete user request received for user ID: {UserId}", id);
            try
            {
                var result = await _userManagementRepository.DeleteUserAsync(id);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[UserManagementController.DeleteUser] - User deleted successfully: {UserId}", id);
                    return SaveSuccess(result.Data);
                }

                if (result.Message == "Không tìm thấy người dùng")
                {
                    _logger.LogWarning("[UserManagementController.DeleteUser] - User not found for deletion with ID: {UserId}", id);
                    return GetError(result.Message);
                }

                _logger.LogWarning("[UserManagementController.DeleteUser] - User deletion failed: {UserId}, Message: {Message}", id, result.Message);
                return Error(result.Message, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserManagementController.DeleteUser] - Error occurred while deleting user with ID: {UserId}", id);
                return Error("Đã xảy ra lỗi khi xóa người dùng", ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách vai trò của người dùng
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các vai trò đã được gán cho một người dùng cụ thể.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     "Nhân viên kỹ thuật",  -> |Tên vai trò|
        ///     "Quản lý kho"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của người dùng cần xem vai trò</param>
        /// <returns>Danh sách vai trò của người dùng</returns>
        /// <response code="200">Thành công, trả về danh sách vai trò</response>
        /// <response code="404">Không tìm thấy người dùng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("users/{id}/roles")]
        [Permission(Permissions.ViewUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            _logger.LogInformation("[UserManagementController.GetUserRoles] - Get roles request received for user ID: {UserId}", id);
            try
            {
                var roles = await _userManagementRepository.GetUserRolesAsync(id);
                _logger.LogInformation("[UserManagementController.GetUserRoles] - Successfully retrieved {Count} roles for user ID: {UserId}", roles.Count(), id);
                return GetSuccess(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserManagementController.GetUserRoles] - Error occurred while getting roles for user with ID: {UserId}", id);
                return GetError("Đã xảy ra lỗi khi lấy vai trò của người dùng: " + ex.Message);
            }
        }

        /// <summary>
        /// Gán vai trò cho người dùng
        /// </summary>
        /// <remarks>
        /// API này thực hiện gán một hoặc nhiều vai trò cho một người dùng.
        /// Các vai trò hiện tại của người dùng sẽ bị ghi đè bởi danh sách vai trò mới.
        /// 
        /// Lưu ý: Chức năng này cũng có thể được thực hiện thông qua API Cập nhật thông tin người dùng (/api/user-management/users/{id}).
        /// API này được giữ lại để tương thích ngược với các ứng dụng hiện có.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// [
        ///   "Quản trại",  -> |Tên vai trò cần gán|
        ///   "Trợ lý"
        /// ]
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "5f8d3b7e-d8f9-4e7c-9d7a-b8c3d4e5f6g7",
        ///     "userName": "nguyenvana",
        ///     "email": "nguyenvana@example.com",
        ///     "phoneNumber": "0912345678",
        ///     "isLocked": false,
        ///     "roles": [
        ///       "Quản trại",
        ///       "Trợ lý"
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của người dùng cần gán vai trò</param>
        /// <param name="roles">Danh sách tên các vai trò cần gán</param>
        /// <returns>Thông tin người dùng sau khi gán vai trò</returns>
        /// <response code="200">Thành công, đã gán vai trò cho người dùng</response>
        /// <response code="404">Không tìm thấy người dùng hoặc vai trò với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("users/{id}/roles")]
        [Permission(Permissions.EditUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignRolesToUser(string id, [FromBody] List<string> roles)
        {
            _logger.LogInformation("[UserManagementController.AssignRolesToUser] - Assign roles request received for user ID: {UserId}, Roles: {@Roles}", id, roles);
            try
            {
                var result = await _userManagementRepository.AssignRolesToUserAsync(id, roles);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[UserManagementController.AssignRolesToUser] - Roles assigned successfully to user ID: {UserId}", id);
                    return SaveSuccess(result.Data);
                }

                _logger.LogWarning("[UserManagementController.AssignRolesToUser] - Assign roles failed for user: {UserId}, Message: {Message}", id, result.Message);
                return Error(result.Message, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserManagementController.AssignRolesToUser] - Error occurred while assigning roles to user with ID: {UserId}", id);
                return Error("Đã xảy ra lỗi khi gán vai trò cho người dùng", ex.Message);
            }
        }


    }
}
