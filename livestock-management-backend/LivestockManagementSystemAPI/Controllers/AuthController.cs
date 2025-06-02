using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using LivestockManagementSystemAPI.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace LivestockManagementSystemAPI.Controllers
{
    /// <summary>
    /// API xác thực và quản lý đăng nhập người dùng
    /// </summary>
    [Route("api/auth")]
    [AllowAnonymous]
    [SwaggerTag("Xác thực và đăng nhập: đăng nhập hệ thống, đăng nhập Google, quên mật khẩu")]
    public class AuthController : BaseAPIController
    {
        private IUserRepository _userRepository { get; set; }
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly TokenSessionService _tokenSessionService;
        private readonly ILogger<AuthController> _logger;
        private readonly HttpClient _httpClient;

        public AuthController(
            IUserRepository userRepository,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            TokenSessionService tokenSessionService,
            ILogger<AuthController> logger,
            HttpClient httpClient)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenSessionService = tokenSessionService;
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Đăng nhập vào hệ thống
        /// </summary>
        /// <remarks>
        /// API này thực hiện xác thực người dùng bằng tên đăng nhập và mật khẩu.
        /// Khi xác thực thành công, API trả về JWT token để sử dụng cho các API khác.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "username": "admin",
        ///   "password": "Admin@123"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///   "errors": null,
        ///   "message": "Đăng nhập thành công"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "statusCode": 400,
        ///   "success": false,
        ///   "data": null,
        ///   "errors": null,
        ///   "message": "Tên đăng nhập hoặc mật khẩu không chính xác"
        /// }
        /// ```
        /// </remarks>
        /// <param name="model">Thông tin đăng nhập</param>
        /// <returns>JWT token khi xác thực thành công</returns>
        /// <response code="200">Xác thực thành công, trả về JWT token</response>
        /// <response code="400">Đăng nhập thất bại, thông tin không chính xác</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginVM model)
        {
            try
            {
                _logger.LogInformation("[AuthController.Login] - Login attempt received for user: {Username}", model.Username);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    _logger.LogWarning("[AuthController.Login] - Login validation failed: {Errors}", string.Join(" | ", errors));
                    return Error(string.Join(" | ", errors));
                }

                var result = await _userRepository.Login(model);
                if (result.IsSuccess && result.Data != null)
                {
                    // Return token directly to client
                    string token = result.Data.ToString();
                    _logger.LogInformation("[AuthController.Login] - User {Username} logged in successfully", model.Username);
                    return Success(token, "Đăng nhập thành công");
                }

                _logger.LogWarning("[AuthController.Login] - Login failed for user {Username}: {Message}", model.Username, result.Message);
                return Error(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController.Login] - Error during login for user {Username}", model.Username);
                return Error("Lỗi xảy ra trong quá trình gửi yêu cầu.", ex);
            }
        }

        /// <summary>
        /// Khởi tạo đăng nhập bằng Google
        /// </summary>
        /// <remarks>
        /// API này chuyển hướng người dùng đến trang đăng nhập Google.
        /// Sau khi xác thực thành công trên Google, người dùng sẽ được chuyển hướng trở lại
        /// endpoint google-callback của ứng dụng với authorization code.
        /// </remarks>
        /// <returns>Chuyển hướng đến trang đăng nhập Google</returns>
        /// <response code="302">Chuyển hướng đến trang đăng nhập Google</response>
        [HttpGet("google-login")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public IActionResult GoogleLogin()
        {

            // Xây dựng URL trực tiếp đến Google
            string clientId = _configuration["Authentication:Google:ClientId"];
            string redirectUri = Url.Action("GoogleCallback", "Auth", null, Request.Scheme, Request.Host.Value);

            _logger.LogInformation("[AuthController.GoogleLogin] - Redirecting to Google OAuth with redirect URI: {RedirectUri}", redirectUri);

            // Chuyển hướng người dùng trực tiếp đến URL đăng nhập của Google
            string googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}" +
                                  $"&response_type=code&scope=openid%20email%20profile&redirect_uri={Uri.EscapeDataString(redirectUri)}";

            return Redirect(googleAuthUrl);
        }

        /// <summary>
        /// Xử lý callback từ Google sau khi đăng nhập
        /// </summary>
        /// <remarks>
        /// API này xử lý callback từ Google sau khi người dùng đăng nhập thành công.
        /// Endpoint này không được gọi trực tiếp bởi client, mà được gọi bởi Google
        /// sau khi người dùng đăng nhập thành công trên trang Google.
        /// 
        /// Khi xác thực thành công với Google, API sẽ:
        /// 1. Lấy thông tin người dùng từ Google
        /// 2. Tạo JWT token
        /// 3. Chuyển hướng người dùng về ứng dụng với auth token trên URL
        /// </remarks>
        /// <param name="code">Mã xác thực từ Google</param>
        /// <returns>Chuyển hướng về ứng dụng với token hoặc thông báo lỗi</returns>
        /// <response code="302">Chuyển hướng về ứng dụng với token hoặc thông báo lỗi</response>
        [HttpGet("google-callback")]
        // [ApiExplorerSettings(IgnoreApi = true)] Ẩn khỏi tài liệu Swagger vì đây là API callback
        public async Task<IActionResult> GoogleCallback(string code)
        {
            _logger.LogInformation("[AuthController.GoogleCallback] - Google callback received with code: {CodePresent}", !string.IsNullOrEmpty(code));

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("[AuthController.GoogleCallback] - Google callback received without code");
                return Redirect($"{_configuration["ClientAppUrl"]}/login?error=no_code");
            }

            try
            {
                // 1. Đổi code lấy token
                string clientId = _configuration["Authentication:Google:ClientId"];
                string clientSecret = _configuration["Authentication:Google:ClientSecret"];
                string redirectUri = Url.Action("GoogleCallback", "Auth", null, Request.Scheme, Request.Host.Value);

                //using var httpClient = new HttpClient();
                var tokenResponse = await _httpClient.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["client_id"] = clientId,
                        ["client_secret"] = clientSecret,
                        ["code"] = code,
                        ["grant_type"] = "authorization_code",
                        ["redirect_uri"] = redirectUri
                    })
                );

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(tokenContent);

                // 2. Lấy thông tin người dùng từ token
                string accessToken = tokenData["access_token"].ToString();
                var userInfoResponse = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                var userData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userInfoContent);

                // 3. Xử lý thông tin người dùng và tạo JWT
                string email = userData["email"].ToString();
                string name = userData["name"]?.ToString() ?? "";
                string googleId = userData["id"].ToString();
                string image = userData["picture"]?.ToString() ?? "";

                _logger.LogInformation("[AuthController.GoogleCallback] - Retrieved Google user info for email: {Email}", email);

                // 4. Gọi service để xử lý đăng nhập và tạo JWT
                var response = await _userRepository.ProcessGoogleLoginAsync(email, name, googleId);

                if (response.IsSuccess)
                {
                    var token = response.Data?.ToString();

                    // Redirect to auth-callback instead of login to properly handle the token
                    _logger.LogInformation("[AuthController.GoogleCallback] - Google login successful for user: {Email}, redirecting to auth-callback", email);
                    return Redirect($"{_configuration["ClientAppUrl"]}/auth-callback?token={Uri.EscapeDataString(token)}");
                }
                else
                {
                    _logger.LogWarning("[AuthController.GoogleCallback] - Google login processing failed: {Message}", response.Message);
                    return Redirect($"{_configuration["ClientAppUrl"]}/login?error={Uri.EscapeDataString(response.Message)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController.GoogleCallback] - Error processing Google callback");
                return Redirect($"{_configuration["ClientAppUrl"]}/login?error={Uri.EscapeDataString(ex.Message)}");
            }
        }

        /// <summary>
        /// Lấy token từ session ID (DEPRECATED)
        /// </summary>
        /// <remarks>
        /// API này đã bị loại bỏ (deprecated). Vui lòng sử dụng API login trực tiếp để nhận token.
        /// 
        /// Ví dụ Response khi gọi API đã bị loại bỏ:
        /// ```json
        /// {
        ///   "statusCode": 400,
        ///   "success": false,
        ///   "data": null,
        ///   "errors": null,
        ///   "message": "API này đã bị loại bỏ, vui lòng sử dụng API login trực tiếp"
        /// }
        /// ```
        /// </remarks>
        /// <param name="sessionId">ID phiên đăng nhập (không còn được sử dụng)</param>
        /// <returns>Thông báo lỗi về API đã bị loại bỏ</returns>
        /// <response code="400">API đã bị loại bỏ, không nên sử dụng</response>
        [HttpGet("RetrieveToken")]
        [Obsolete("This method is deprecated. Use direct token response from login instead.")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiExplorerSettings(IgnoreApi = false)] // Vẫn hiển thị trong Swagger để cảnh báo người dùng
        public IActionResult RetrieveToken(string sessionId)
        {
            _logger.LogWarning("[AuthController.RetrieveToken] - Deprecated endpoint called with sessionId: {SessionIdPresent}", !string.IsNullOrEmpty(sessionId));
            return Error("API này đã bị loại bỏ, vui lòng sử dụng API login trực tiếp");
        }

        /// <summary>
        /// Khởi tạo quá trình quên mật khẩu
        /// </summary>
        /// <remarks>
        /// API này khởi tạo quá trình quên mật khẩu, gửi mã xác thực đến email hoặc số điện thoại người dùng tuỳ thuộc vào người dùng nhập gì.
        /// Khi có thông báo gửi thành công sẽ trả về một resetToken, expiresAt và contactType. 
        /// - expiresAt là thời gian hạn sử dụng của resetToken.  
        /// - contactType là loại liên hệ mà người dùng đã nhập vào, có thể là email hoặc số điện thoại.
        /// - resetToken là một mã trả về. Mã này sẽ được truyền trong API completeForgotPassword (dùng để lấy code và thông tin người dùng, phải được gửi đi cùng form completeForgotPassword)
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "contactInfo": "09122831358"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "data": {
        ///     "resetToken": "24a5ee7d9ca048d188e81cb2dc760918",
        ///     "expiresAt": "2025-03-30T15:08:51.3892443Z",
        ///     "contactType": "Email"
        ///   },
        ///   "errors": null,
        ///   "message": "Mã xác nhận đã được gửi đến email: dt*******@gmail.com"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "statusCode": 400,
        ///   "success": false,
        ///   "data": null,
        ///   "errors": null,
        ///   "message": "Định dạng thông tin liên hệ không hợp lệ. Vui lòng nhập email hoặc số điện thoại hợp lệ."
        /// }
        /// ```
        /// </remarks>
        /// <param name="model">Thông tin liên hệ của người dùng (email hoặc số điện thoại)</param>
        /// <returns>Kết quả khởi tạo quá trình quên mật khẩu</returns>
        /// <response code="200">Thành công, đã gửi mã xác thực</response>
        /// <response code="400">Thất bại, không tìm thấy tài khoản hoặc thông tin không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("forgot-password/initiate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitiateForgotPassword([FromBody] InitiateForgotPasswordDTO model)
        {
            _logger.LogInformation("[AuthController.InitiateForgotPassword] - Request received for contact info");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("[AuthController.InitiateForgotPassword] - Validation failed: {Errors}", errors);
                return Error(errors);
            }

            try
            {
                var result = await _userRepository.InitiateForgotPasswordAsync(model);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("[AuthController.InitiateForgotPassword] - Verification code sent successfully");
                    return Success(result.Data, result.Message);
                }

                _logger.LogWarning("[AuthController.InitiateForgotPassword] - Failed to send verification code: {Message}", result.Message);
                return Error(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController.InitiateForgotPassword] - Error sending verification code");
                return Error("Đã xảy ra lỗi khi xử lý yêu cầu quên mật khẩu.");
            }
        }

        /// <summary>
        /// Hoàn thành quá trình quên mật khẩu
        /// </summary>
        /// <remarks>
        /// API này xác nhận mã OTP và cập nhật mật khẩu mới cho người dùng.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "resetToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
        ///   "verificationCode": "123456",
        ///   "newPassword": "NewPassword@123",
        ///   "confirmNewPassword": "NewPassword@123"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "data": null,
        ///   "errors": null,
        ///   "message": "Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới."
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "statusCode": 400,
        ///   "success": false,
        ///   "data": null,
        ///   "errors": null,
        ///   "message": "Mã xác nhận không chính xác hoặc đã hết hạn"
        /// }
        /// ```
        /// </remarks>
        /// <param name="model">Thông tin xác nhận và mật khẩu mới</param>
        /// <returns>Kết quả cập nhật mật khẩu</returns>
        /// <response code="200">Thành công, đã cập nhật mật khẩu mới</response>
        /// <response code="400">Thất bại, mã xác nhận không hợp lệ hoặc đã hết hạn</response>
        /// <response code="404">Không tìm thấy tài khoản với thông tin liên hệ cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("forgot-password/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompleteForgotPassword([FromBody] CompleteForgotPasswordDTO model)
        {
            _logger.LogInformation("[AuthController.CompleteForgotPassword] - Request received");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("[AuthController.CompleteForgotPassword] - Validation failed: {Errors}", errors);
                return Error(errors);
            }

            try
            {
                var result = await _userRepository.CompleteForgotPasswordAsync(model);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("[AuthController.CompleteForgotPassword] - Password reset successfully");
                    return Success(null, result.Message);
                }

                _logger.LogWarning("[AuthController.CompleteForgotPassword] - Failed to reset password: {Message}", result.Message);
                return Error(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController.CompleteForgotPassword] - Error occurred during password reset");
                return Error("Đã xảy ra lỗi khi đặt lại mật khẩu. Vui lòng thử lại sau.");
            }
        }
    }
}
