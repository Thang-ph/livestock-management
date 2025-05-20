using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Quản lý người dùng: quản lý thông tin cá nhân và thay đổi mật khẩu")]
    public class UserController : BaseAPIController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin cá nhân của người dùng hiện tại
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin cá nhân của người dùng hiện tại dựa trên token xác thực.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "5f8d3b7e-d8f9-4e7c-9d7a-b8c3d4e5f6g7",  -> |ID của người dùng|
        ///     "userName": "nguyenvana",                      -> |Tên đăng nhập|
        ///     "email": "nguyenvana@example.com",             -> |Địa chỉ email|
        ///     "phoneNumber": "0912345678",                   -> |Số điện thoại|
        ///     "roles": [                                    -> |Danh sách vai trò của người dùng|
        ///       "Nhân viên kỹ thuật",
        ///       "Quản lý kho"
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Thông tin cá nhân của người dùng hiện tại</returns>
        /// <response code="200">Thành công, trả về thông tin người dùng</response>
        /// <response code="400">Không tìm thấy người dùng trong token</response>
        /// <response code="404">Không tìm thấy thông tin cá nhân của người dùng</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("my-profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            _logger.LogInformation("[{ControllerName}.{MethodName}] - Request received",
                this.GetType().Name, nameof(GetCurrentUserProfile));
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("[{ControllerName}.{MethodName}] - User ID not found in claims",
                        this.GetType().Name, nameof(GetCurrentUserProfile));
                    return GetError("Không tìm thấy thông tin người dùng");
                }

                var profile = await _userRepository.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    _logger.LogWarning("[{ControllerName}.{MethodName}] - Profile not found for user ID: {UserId}",
                        this.GetType().Name, nameof(GetCurrentUserProfile), userId);
                    return GetError("Không tìm thấy thông tin cá nhân");
                }

                _logger.LogInformation("[{ControllerName}.{MethodName}] - Successfully retrieved profile for user ID: {UserId}",
                    this.GetType().Name, nameof(GetCurrentUserProfile), userId);
                return GetSuccess(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ControllerName}.{MethodName}] - Error retrieving profile",
                    this.GetType().Name, nameof(GetCurrentUserProfile));
                return GetError("Đã xảy ra lỗi khi lấy thông tin cá nhân: " + ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân của người dùng hiện tại
        /// </summary>
        /// <remarks>
        /// API này thực hiện cập nhật thông tin cá nhân của người dùng hiện tại.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "userName": "nguyenvana_updated",     -> |Tên đăng nhập mới|
        ///   "email": "nguyenvana_new@example.com", -> |Địa chỉ email mới|
        ///   "phoneNumber": "0987654321"           -> |Số điện thoại mới|
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
        ///     "roles": [
        ///       "Nhân viên kỹ thuật",
        ///       "Quản lý kho"
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="model">Thông tin cá nhân cần cập nhật</param>
        /// <returns>Thông tin cá nhân sau khi cập nhật</returns>
        /// <response code="200">Thành công, đã cập nhật thông tin người dùng</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc không tìm thấy người dùng</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("update-profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UpdateProfileDTO model)
        {
            _logger.LogInformation("[{ControllerName}.{MethodName}] - Request received",
                this.GetType().Name, nameof(UpdateCurrentUserProfile));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                _logger.LogWarning("[{ControllerName}.{MethodName}] - Validation failed: {Errors}",
                    this.GetType().Name, nameof(UpdateCurrentUserProfile), string.Join(" | ", errors));
                return Error(string.Join(" | ", errors));
            }

            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("[{ControllerName}.{MethodName}] - User ID not found in claims",
                        this.GetType().Name, nameof(UpdateCurrentUserProfile));
                    return GetError("Không tìm thấy thông tin người dùng");
                }

                var result = await _userRepository.UpdateUserProfileAsync(userId, model);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[{ControllerName}.{MethodName}] - Successfully updated profile for user ID: {UserId}",
                        this.GetType().Name, nameof(UpdateCurrentUserProfile), userId);
                    return SaveSuccess(result.Data);
                }

                _logger.LogWarning("[{ControllerName}.{MethodName}] - Profile update failed: {Message}",
                    this.GetType().Name, nameof(UpdateCurrentUserProfile), result.Message);
                return Error(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ControllerName}.{MethodName}] - Error updating profile",
                    this.GetType().Name, nameof(UpdateCurrentUserProfile));
                return Error("Đã xảy ra lỗi khi cập nhật thông tin cá nhân", ex.Message);
            }
        }

        /// <summary>
        /// Khởi tạo quá trình đổi mật khẩu
        /// </summary>
        /// <remarks>
        /// API này khởi tạo quá trình đổi mật khẩu bằng cách gửi mã xác thực đến email hoặc số điện thoại của người dùng.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "verificationMethod": "Email"     -> |Phương thức xác thực: "Email" hoặc "Phone"|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": {
        ///     "verificationSent": true        -> |Trạng thái gửi mã xác thực|
        ///   },
        ///   "message": "Mã xác thực đã được gửi đến email của bạn"
        /// }
        /// ```
        /// </remarks>
        /// <param name="model">Thông tin phương thức xác thực</param>
        /// <returns>Kết quả khởi tạo quá trình đổi mật khẩu</returns>
        /// <response code="200">Thành công, đã gửi mã xác thực</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc không tìm thấy người dùng</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("change-password/initiate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitiatePasswordChange([FromBody] InitiatePasswordChangeDTO model)
        {
            _logger.LogInformation("[{ControllerName}.{MethodName}] - Request received",
                this.GetType().Name, nameof(InitiatePasswordChange));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                _logger.LogWarning("[{ControllerName}.{MethodName}] - Validation failed: {Errors}",
                    this.GetType().Name, nameof(InitiatePasswordChange), string.Join(" | ", errors));
                return Error(string.Join(" | ", errors));
            }

            try
            {
                // Get current user ID from claims - kiểm tra nhiều dạng claim type
                var userId = User.Claims.FirstOrDefault(c => c.Type.Equals("UserId"))?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;


                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("[{ControllerName}.{MethodName}] - User ID not found in claims",
                        this.GetType().Name, nameof(InitiatePasswordChange));
                    return GetError("Không tìm thấy thông tin người dùng");
                }

                // Call service to initiate password change
                var result = await _userRepository.InitiatePasswordChangeAsync(userId, model);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[{ControllerName}.{MethodName}] - Verification code sent for user ID: {UserId}",
                        this.GetType().Name, nameof(InitiatePasswordChange), userId);
                    return Success(result.Data, result.Message);
                }

                _logger.LogWarning("[{ControllerName}.{MethodName}] - Failed to initiate password change: {Message}",
                    this.GetType().Name, nameof(InitiatePasswordChange), result.Message);
                return Error(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ControllerName}.{MethodName}] - Error initiating password change",
                    this.GetType().Name, nameof(InitiatePasswordChange));
                return Error("Đã xảy ra lỗi khi khởi tạo quá trình đổi mật khẩu", ex.Message);
            }
        }

        /// <summary>
        /// Hoàn tất quá trình đổi mật khẩu
        /// </summary>
        /// <remarks>
        /// API này hoàn tất quá trình đổi mật khẩu bằng cách xác thực mã và cập nhật mật khẩu mới.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "verificationCode": "123456",      -> |Mã xác thực đã nhận|
        ///   "newPassword": "NewPass@123",      -> |Mật khẩu mới|
        ///   "confirmNewPassword": "NewPass@123" -> |Xác nhận lại mật khẩu mới|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "message": "Mật khẩu đã được thay đổi thành công"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "message": "Mã xác thực không đúng hoặc đã hết hạn"
        /// }
        /// ```
        /// </remarks>
        /// <param name="model">Thông tin xác thực và mật khẩu mới</param>
        /// <returns>Kết quả đổi mật khẩu</returns>
        /// <response code="200">Thành công, đã đổi mật khẩu</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ, mã xác thực không đúng, hoặc mật khẩu không khớp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("change-password/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompletePasswordChange([FromBody] CompletePasswordChangeDTO model)
        {
            _logger.LogInformation("[{ControllerName}.{MethodName}] - Request received",
                this.GetType().Name, nameof(CompletePasswordChange));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                _logger.LogWarning("[{ControllerName}.{MethodName}] - Validation failed: {Errors}",
                    this.GetType().Name, nameof(CompletePasswordChange), string.Join(" | ", errors));
                return Error(string.Join(" | ", errors));
            }

            try
            {
                // Get current user ID from claims
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                          ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("[{ControllerName}.{MethodName}] - User ID not found in claims",
                        this.GetType().Name, nameof(CompletePasswordChange));
                    return GetError("Không tìm thấy thông tin người dùng");
                }

                // Call service to complete password change
                var result = await _userRepository.CompletePasswordChangeAsync(userId, model);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[{ControllerName}.{MethodName}] - Password changed successfully for user ID: {UserId}",
                        this.GetType().Name, nameof(CompletePasswordChange), userId);
                    return Success(null, result.Message);
                }

                _logger.LogWarning("[{ControllerName}.{MethodName}] - Failed to change password: {Message}",
                    this.GetType().Name, nameof(CompletePasswordChange), result.Message);
                return Error(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ControllerName}.{MethodName}] - Error changing password",
                    this.GetType().Name, nameof(CompletePasswordChange));
                return Error("Đã xảy ra lỗi khi đổi mật khẩu", ex.Message);
            }
        }


        #region helper methods
        // Helper method to get the current user's ID from claims
        private string GetCurrentUserId()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                      ?? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                      ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            return userId;
        }
        #endregion
    }
}
