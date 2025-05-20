using AutoMapper;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;

namespace DataAccess.Repository.Services
{
    public class UserService : IUserRepository
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtSetting _jwtSetting;
        private readonly LmsContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly IPermissionRepository _permissionRepository;
        // Dictionary to store verification codes temporarily
        // Key: UserId, Value: (Code, ExpiryTime)
        private static readonly Dictionary<string, (string Code, DateTime ExpiryTime)> _verificationCodes = new Dictionary<string, (string, DateTime)>();

        // Dictionary to store reset password verification codes temporarily
        // Key: Token, Value: (ContactInfo, Code, ExpiryTime, UserId)
        private static readonly Dictionary<string, (string ContactInfo, string Code, DateTime ExpiryTime, string UserId)> _resetPasswordTokens =
            new Dictionary<string, (string, string, DateTime, string)>();

        public UserService(UserManager<IdentityUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                          SignInManager<IdentityUser> signInManager,
                          LmsContext context,
                          IMapper mapper,
                          IOptionsMonitor<JwtSetting> jwtSetting,
                          IConfiguration configuration,
                          ILogger<UserService> logger,
                          ISmsService smsService,
                          IEmailService emailService,
                          IPermissionRepository permissionRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _jwtSetting = jwtSetting.CurrentValue;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _smsService = smsService;
            _emailService = emailService;
            _permissionRepository = permissionRepository;
        }


        public async Task<ResponseModel> Login(LoginVM model)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - User login attempt with username: {Username}",
                nameof(UserService), nameof(Login), model.Username);

            IdentityUser user = await _userManager.FindByEmailAsync(model.Username)
                ?? await FindByPhoneNumberAsync(model.Username);

            if (user == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Login failed: User not found for username {Username}",
                    nameof(UserService), nameof(Login), model.Username);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Login failed: Account locked for user {Username}",
                    nameof(UserService), nameof(Login), model.Username);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Tài khoản của bạn đã bị khoá. Vui lòng thử lại sau!"
                };
            }

            // Thay thế SignInManager bằng UserManager để kiểm tra mật khẩu mà không tạo cookie
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!isPasswordValid)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Login failed: Invalid password for user {Username}",
                    nameof(UserService), nameof(Login), model.Username);

                // Tăng số lần đăng nhập thất bại (có thể dẫn đến khóa tài khoản)
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Tài khoản của bạn đã bị khoá do đăng nhập thất bại nhiều lần."
                    };
                }

                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                };
            }

            // Reset đếm số lần đăng nhập thất bại khi đăng nhập thành công
            await _userManager.ResetAccessFailedCountAsync(user);

            _logger.LogInformation("[{ServiceName}.{MethodName}] - User {Username} logged in successfully",
                nameof(UserService), nameof(Login), model.Username);
            var accessToken = await GenerateJwtToken(user);
            return new ResponseModel
            {
                IsSuccess = true,
                Data = accessToken
            };
        }

        public async Task<ResponseModel> ProcessGoogleLoginAsync(string email, string name, string googleId)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Processing Google login for email: {Email}",
                nameof(UserService), nameof(ProcessGoogleLoginAsync), email);

            // Kiểm tra xem email đã tồn tại chưa
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Google login failed: User with email {Email} not found in system",
                    nameof(UserService), nameof(ProcessGoogleLoginAsync), email);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Tài khoản không được phép truy cập hoặc không tồn tại trong hệ thống"
                };
            }

            // Kiểm tra xem tài khoản có bị khóa không
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Google login failed: Account locked for user with email {Email}",
                    nameof(UserService), nameof(ProcessGoogleLoginAsync), email);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên."
                };
            }

            // Reset đếm số lần đăng nhập thất bại khi đăng nhập thành công
            await _userManager.ResetAccessFailedCountAsync(user);

            _logger.LogInformation("[{ServiceName}.{MethodName}] - Google login successful for user with email {Email}",
                nameof(UserService), nameof(ProcessGoogleLoginAsync), email);
            var accessToken = await GenerateJwtToken(user);
            return new ResponseModel
            {
                IsSuccess = true,
                Data = accessToken
            };
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(string userId)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Getting profile for user ID: {UserId}",
                nameof(UserService), nameof(GetUserProfileAsync), userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - User not found with ID: {UserId}",
                    nameof(UserService), nameof(GetUserProfileAsync), userId);
                return null;
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Map user to profile DTO - đã loại bỏ các trường xác thực
            var profileDto = new UserProfileDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList()
            };

            return profileDto;
        }

        public async Task<ResponseModel> UpdateUserProfileAsync(string userId, UpdateProfileDTO model)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Updating profile for user ID: {UserId}",
                nameof(UserService), nameof(UpdateUserProfileAsync), userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - User not found with ID: {UserId}",
                    nameof(UserService), nameof(UpdateUserProfileAsync), userId);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người dùng"
                };
            }

            // Validate email uniqueness if changing email
            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    _logger.LogWarning("[{ServiceName}.{MethodName}] - Email {Email} already in use",
                        nameof(UserService), nameof(UpdateUserProfileAsync), model.Email);
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Email đã được sử dụng bởi tài khoản khác"
                    };
                }

                user.Email = model.Email;
                // Vẫn giữ lại việc xác thực email ở phía backend để đảm bảo tính nhất quán
                user.EmailConfirmed = true;
            }

            // Validate phone uniqueness if changing phone
            if (!string.IsNullOrEmpty(model.PhoneNumber) && model.PhoneNumber != user.PhoneNumber)
            {
                var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != userId);
                if (existingUser != null)
                {
                    _logger.LogWarning("[{ServiceName}.{MethodName}] - Phone number {Phone} already in use",
                        nameof(UserService), nameof(UpdateUserProfileAsync), model.PhoneNumber);
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Số điện thoại đã được sử dụng bởi tài khoản khác"
                    };
                }

                user.PhoneNumber = model.PhoneNumber;
                // Vẫn giữ lại việc xác thực số điện thoại ở phía backend để đảm bảo tính nhất quán
                user.PhoneNumberConfirmed = true;
            }

            // Update username if provided - đã loại bỏ phần kiểm tra username unique
            if (!string.IsNullOrEmpty(model.UserName) && model.UserName != user.UserName)
            {
                user.UserName = model.UserName;
            }

            // Save changes
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("[{ServiceName}.{MethodName}] - Failed to update user: {Errors}",
                    nameof(UserService), nameof(UpdateUserProfileAsync), errors);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = errors
                };
            }

            _logger.LogInformation("[{ServiceName}.{MethodName}] - Successfully updated profile for user ID: {UserId}",
                nameof(UserService), nameof(UpdateUserProfileAsync), userId);
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Cập nhật thông tin cá nhân thành công",
                Data = await GetUserProfileAsync(userId)
            };
        }

        public async Task<ResponseModel> InitiatePasswordChangeAsync(string userId, InitiatePasswordChangeDTO model)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Initiating password change for user ID: {UserId}",
                nameof(UserService), nameof(InitiatePasswordChangeAsync), userId);

            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - User not found with ID: {UserId}",
                    nameof(UserService), nameof(InitiatePasswordChangeAsync), userId);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người dùng"
                };
            }

            // Generate a random 6-digit verification code
            var verificationCode = GenerateVerificationCode();

            // Set expiration time (15 minutes from now)
            var expiryTime = DateTime.UtcNow.AddMinutes(15);

            // Store the verification code with expiry time
            lock (_verificationCodes)
            {
                _verificationCodes[userId] = (verificationCode, expiryTime);
            }

            // Send verification code based on chosen method
            bool codeSent = false;
            string destination = "";

            if (model.VerificationMethod.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                // Send code to email
                if (string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning("[{ServiceName}.{MethodName}] - User has no email address: {UserId}",
                        nameof(UserService), nameof(InitiatePasswordChangeAsync), userId);
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Người dùng không có địa chỉ email"
                    };
                }

                codeSent = await SendEmailVerificationCode(user.Email, verificationCode);
                destination = user.Email;
            }
            else if (model.VerificationMethod.Equals("Phone", StringComparison.OrdinalIgnoreCase))
            {
                // Send code to phone
                if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    _logger.LogWarning("[{ServiceName}.{MethodName}] - User has no phone number: {UserId}",
                        nameof(UserService), nameof(InitiatePasswordChangeAsync), userId);
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Người dùng không có số điện thoại"
                    };
                }

                codeSent = await SendSmsVerificationCode(user.PhoneNumber, verificationCode);

                if (!codeSent)
                {
                    _logger.LogWarning("[{ServiceName}.{MethodName}] - Failed to send SMS to user: {UserId}",
                        nameof(UserService), nameof(InitiatePasswordChangeAsync), userId);

                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Không thể gửi tin nhắn SMS. Vui lòng thử lại sau hoặc chọn phương thức xác thực khác."
                    };
                }

                destination = user.PhoneNumber;
            }
            else
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Invalid verification method: {Method}",
                    nameof(UserService), nameof(InitiatePasswordChangeAsync), model.VerificationMethod);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Phương thức xác thực không hợp lệ. Vui lòng chọn 'Email' hoặc 'Phone'."
                };
            }

            if (!codeSent)
            {
                _logger.LogError("[{ServiceName}.{MethodName}] - Failed to send verification code for user: {UserId}",
                    nameof(UserService), nameof(InitiatePasswordChangeAsync), userId);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không thể gửi mã xác nhận. Vui lòng thử lại sau."
                };
            }

            _logger.LogInformation("[{ServiceName}.{MethodName}] - Verification code sent successfully to user: {UserId}",
                nameof(UserService), nameof(InitiatePasswordChangeAsync), userId);
            return new ResponseModel
            {
                IsSuccess = true,
                Message = $"Mã xác nhận đã được gửi đến {model.VerificationMethod.ToLower()} của bạn: {MaskDestination(destination, model.VerificationMethod)}",
                Data = new { ExpiresAt = expiryTime }
            };
        }

        public async Task<ResponseModel> CompletePasswordChangeAsync(string userId, CompletePasswordChangeDTO model)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Completing password change for user ID: {UserId}",
                nameof(UserService), nameof(CompletePasswordChangeAsync), userId);

            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - User not found with ID: {UserId}",
                    nameof(UserService), nameof(CompletePasswordChangeAsync), userId);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người dùng"
                };
            }

            // Validate verification code
            var isCodeValid = false;
            var codeData = (Code: "", ExpiryTime: DateTime.MinValue);
            var codeExists = false;

            lock (_verificationCodes)
            {
                if (_verificationCodes.TryGetValue(userId, out codeData))
                {
                    codeExists = true;
                    // Check if code matches and is not expired
                    if (codeData.Code == model.VerificationCode && codeData.ExpiryTime > DateTime.UtcNow)
                    {
                        isCodeValid = true;
                        // Không xóa mã ở đây, sẽ xóa sau khi đổi mật khẩu thành công
                    }
                    else if (codeData.ExpiryTime <= DateTime.UtcNow)
                    {
                        // Clean up expired code
                        _verificationCodes.Remove(userId);
                        codeExists = false;
                    }
                }
            }

            if (!isCodeValid)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Invalid or expired verification code for user: {UserId}",
                    nameof(UserService), nameof(CompletePasswordChangeAsync), userId);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = codeExists
                        ? "Mã xác nhận không chính xác"
                        : "Mã xác nhận không hợp lệ hoặc đã hết hạn"
                };
            }

            // Validate password confirmation
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Passwords do not match for user: {UserId}",
                    nameof(UserService), nameof(CompletePasswordChangeAsync), userId);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Mật khẩu xác nhận không khớp với mật khẩu mới"
                };
            }

            // Change password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("[{ServiceName}.{MethodName}] - Failed to change password for user: {UserId}, Errors: {Errors}",
                    nameof(UserService), nameof(CompletePasswordChangeAsync), userId, errors);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = errors
                };
            }

            // Xóa mã xác thực khi đổi mật khẩu thành công
            lock (_verificationCodes)
            {
                _verificationCodes.Remove(userId);
            }

            _logger.LogInformation("[{ServiceName}.{MethodName}] - Password changed successfully for user: {UserId}",
                nameof(UserService), nameof(CompletePasswordChangeAsync), userId);
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Đổi mật khẩu thành công"
            };
        }

        public async Task<ResponseModel> InitiateForgotPasswordAsync(InitiateForgotPasswordDTO model)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Initiating forgot password for contact: {ContactInfo}",
                nameof(UserService), nameof(InitiateForgotPasswordAsync), MaskContactInfo(model.ContactInfo));

            // Determine if the contact info is an email or phone number
            string verificationMethod = DetermineContactType(model.ContactInfo);
            if (string.IsNullOrEmpty(verificationMethod))
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Invalid contact info format: {ContactInfo}",
                    nameof(UserService), nameof(InitiateForgotPasswordAsync), MaskContactInfo(model.ContactInfo));
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Định dạng thông tin liên hệ không hợp lệ. Vui lòng nhập email hoặc số điện thoại hợp lệ."
                };
            }

            // Find user by email or phone
            IdentityUser user = null;
            if (verificationMethod == "Email")
            {
                user = await _userManager.FindByEmailAsync(model.ContactInfo);
            }
            else // Phone
            {
                user = await FindByPhoneNumberAsync(model.ContactInfo);
            }

            if (user == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - User not found with {Method}: {ContactInfo}",
                    nameof(UserService), nameof(InitiateForgotPasswordAsync), verificationMethod, MaskContactInfo(model.ContactInfo));

                // For security reasons, we still return success to avoid leaking information about registered users
                // But we don't actually send any code
                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = $"Nếu {verificationMethod.ToLower()} đã đăng ký trong hệ thống, bạn sẽ nhận được mã xác nhận."
                };
            }

            // Check if the account is locked
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Account is locked: {UserId}",
                    nameof(UserService), nameof(InitiateForgotPasswordAsync), user.Id);

                // For security reasons, we still return success but don't send code
                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = $"Nếu {verificationMethod.ToLower()} đã đăng ký trong hệ thống, bạn sẽ nhận được mã xác nhận."
                };
            }

            // Generate a verification code
            var verificationCode = GenerateVerificationCode();

            // Set expiration time (15 minutes)
            var expiryTime = DateTime.UtcNow.AddMinutes(5);

            // Generate a unique token for this reset password session
            string resetToken = Guid.NewGuid().ToString("N");

            // Store the code with token for later verification
            lock (_resetPasswordTokens)
            {
                _resetPasswordTokens[resetToken] = (model.ContactInfo, verificationCode, expiryTime, user.Id);
            }

            // Send the verification code
            bool codeSent = false;
            string destination = model.ContactInfo;

            if (verificationMethod == "Email")
            {
                codeSent = await SendEmailVerificationCode(destination, verificationCode);
            }
            else // Phone
            {
                codeSent = await SendSmsVerificationCode(destination, verificationCode);
            }

            if (!codeSent)
            {
                _logger.LogError("[{ServiceName}.{MethodName}] - Failed to send verification code to: {ContactInfo}",
                    nameof(UserService), nameof(InitiateForgotPasswordAsync), MaskContactInfo(model.ContactInfo));

                // Remove the stored code
                lock (_resetPasswordTokens)
                {
                    _resetPasswordTokens.Remove(resetToken);
                }

                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không thể gửi mã xác nhận. Vui lòng thử lại sau."
                };
            }

            _logger.LogInformation("[{ServiceName}.{MethodName}] - Verification code sent successfully to: {ContactInfo}",
                nameof(UserService), nameof(InitiateForgotPasswordAsync), MaskContactInfo(model.ContactInfo));

            return new ResponseModel
            {
                IsSuccess = true,
                Message = $"Mã xác nhận đã được gửi đến {verificationMethod.ToLower()}: {MaskDestination(destination, verificationMethod)}",
                Data = new
                {
                    ResetToken = resetToken,
                    ExpiresAt = expiryTime,
                    ContactType = verificationMethod
                }
            };
        }

        public async Task<ResponseModel> CompleteForgotPasswordAsync(CompleteForgotPasswordDTO model)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Completing forgot password for token: {ResetToken}",
                nameof(UserService), nameof(CompleteForgotPasswordAsync), model.ResetToken);

            // Validate the verification code
            string userId = null;
            bool tokenExists = false;

            lock (_resetPasswordTokens)
            {
                if (_resetPasswordTokens.TryGetValue(model.ResetToken, out var tokenData))
                {
                    tokenExists = true;

                    // Check if code matches and is not expired
                    if (tokenData.Code == model.VerificationCode && tokenData.ExpiryTime > DateTime.UtcNow)
                    {
                        userId = tokenData.UserId;
                        // Không xóa token ở đây, sẽ xóa sau khi hoàn thành thành công
                    }
                    else if (tokenData.ExpiryTime <= DateTime.UtcNow)
                    {
                        // Clean up expired code
                        _resetPasswordTokens.Remove(model.ResetToken);
                        tokenExists = false;
                    }
                }
            }

            if (userId == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Invalid or expired verification code for token: {ResetToken}",
                    nameof(UserService), nameof(CompleteForgotPasswordAsync), model.ResetToken);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = tokenExists
                        ? "Mã xác nhận không chính xác"
                        : "Mã xác nhận không hợp lệ hoặc đã hết hạn"
                };
            }

            // Find the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - User not found with ID: {UserId}",
                    nameof(UserService), nameof(CompleteForgotPasswordAsync), userId);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy người dùng"
                };
            }

            // Validate password confirmation
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                _logger.LogWarning("[{ServiceName}.{MethodName}] - Passwords do not match for user: {UserId}",
                    nameof(UserService), nameof(CompleteForgotPasswordAsync), userId);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Mật khẩu xác nhận không khớp với mật khẩu mới"
                };
            }

            // Reset the password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("[{ServiceName}.{MethodName}] - Failed to reset password for user: {UserId}, Errors: {Errors}",
                    nameof(UserService), nameof(CompleteForgotPasswordAsync), userId, errors);
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = errors
                };
            }

            // Thành công, xóa token
            lock (_resetPasswordTokens)
            {
                _resetPasswordTokens.Remove(model.ResetToken);
            }

            // Unlock the account if it was locked
            if (await _userManager.IsLockedOutAsync(user))
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                _logger.LogInformation("[{ServiceName}.{MethodName}] - Account unlocked after password reset: {UserId}",
                    nameof(UserService), nameof(CompleteForgotPasswordAsync), userId);
            }

            _logger.LogInformation("[{ServiceName}.{MethodName}] - Password reset successful for user: {UserId}",
                nameof(UserService), nameof(CompleteForgotPasswordAsync), userId);
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới."
            };
        }

        public async Task<int> GetUserCountInRole(string roleName)
        {
            try
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                return usersInRole.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService.GetUserCountInRole] - Error getting user count in role: {RoleName}", roleName);
                throw;
            }
        }

        #region privateMewthods
        private async Task<List<Claim>> GetClaimUser(IdentityUser user)
        {
            // Lấy danh sách role của user
            var roles = await _userManager.GetRolesAsync(user);

            // Tìm role có priority cao nhất
            string primaryRole = null;
            int highestPriority = int.MaxValue;
            var rolesWithPriority = new List<RoleWithPriority>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                int priority = await _permissionRepository.GetRolePriorityByNameAsync(roleName);
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

            // Sắp xếp roles theo priority tăng dần
            rolesWithPriority = rolesWithPriority.OrderBy(r => r.Priority).ToList();

            var result = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("UserId", user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("PhoneNumber", user.PhoneNumber),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrEmpty(primaryRole))
            {
                result.Add(new Claim("PrimaryRole", primaryRole));
            }

            foreach (var role in roles)
            {
                result.Add(new Claim("Role", role));
                result.Add(new Claim(ClaimTypes.Role, role)); // Thêm định dạng chuẩn

                // Thêm các permissions theo role
                var roleObj = await _roleManager.FindByNameAsync(role);
                if (roleObj != null)
                {
                    // Lấy tất cả claims của role
                    var roleClaims = await _roleManager.GetClaimsAsync(roleObj);
                    // Thêm các permission claims vào token
                    foreach (var claim in roleClaims.Where(c => c.Type == "Permission"))
                    {
                        // Kiểm tra xem claim đã tồn tại chưa để tránh trùng lặp
                        if (!result.Any(c => c.Type == "Permission" && c.Value == claim.Value))
                        {
                            result.Add(new Claim(claim.Type, claim.Value));
                        }
                    }
                }
            }

            // Thêm thông tin phân cấp role vào token dưới dạng danh sách
            foreach (var role in rolesWithPriority)
            {
                result.Add(new Claim("RoleHierarchy", $"{role.Name} : {role.Priority}"));
            }

            return result;
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessTokenExpirationMinutes = double.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"]);
            var refreshTokenExpirationDays = double.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]);
            var claims = await GetClaimUser(user);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
                notBefore: DateTime.UtcNow,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<IdentityUser> FindByPhoneNumberAsync(string phoneNumber)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Finding user by phone number: {PhoneNumber}",
                nameof(UserService), nameof(FindByPhoneNumberAsync), phoneNumber);
            return await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        // Generate a random 6-digit verification code
        private string GenerateVerificationCode()
        {
            // Generate a random 6-digit number
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // Send verification code via email
        private async Task<bool> SendEmailVerificationCode(string email, string code)
        {
            try
            {
                _logger.LogInformation("[{ServiceName}.SendEmailVerificationCode] - Sending verification code via email to: {Email}",
                    nameof(UserService), email);

                return await _emailService.SendVerificationCodeAsync(email, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ServiceName}.SendEmailVerificationCode] - Error sending verification code to email: {Email}",
                    nameof(UserService), email);
                return false;
            }
        }

        // Send verification code via SMS
        private async Task<bool> SendSmsVerificationCode(string phoneNumber, string code)
        {
            try
            {
                // Kiểm tra xem số điện thoại có hợp lệ không
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    return false;
                }

                // Tạo nội dung tin nhắn
                // string message = $"HTX Lúa Vàng - Mã xác nhận của bạn là: {code}. Mã này có hiệu lực trong 5 phút.";
                string messageTest = $"{code} la ma xac minh dang ky Baotrixemay cua ban";

                // Kiểm tra dịch vụ SMS
                if (_smsService == null)
                {
                    _logger.LogError("[{ServiceName}.SendSmsVerificationCode] - SMS service is not configured",
                        nameof(UserService));
                    return false;
                }


                // Gửi SMS qua dịch vụ đã cấu hình
                bool result = await _smsService.SendSmsAsync(phoneNumber, messageTest);

                if (result)
                {
                    _logger.LogInformation("[{ServiceName}.SendSmsVerificationCode] - SMS sent successfully to {Phone}",
                        nameof(UserService), phoneNumber);
                }
                else
                {
                    _logger.LogWarning("[{ServiceName}.SendSmsVerificationCode] - Failed to send SMS to {Phone}",
                        nameof(UserService), phoneNumber);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ServiceName}.SendSmsVerificationCode] - Error sending SMS to {Phone}: {ErrorMessage}",
                    nameof(UserService), phoneNumber, ex.Message);
                return false;
            }
        }

        // Mask email or phone number for privacy in response messages
        private string MaskDestination(string destination, string method)
        {
            if (string.IsNullOrEmpty(destination))
                return string.Empty;

            if (method.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                // Mask email: show first 2 characters and domain, hide the rest with ***
                var parts = destination.Split('@');
                if (parts.Length == 2)
                {
                    var username = parts[0];
                    var domain = parts[1];
                    var maskedUsername = username.Length <= 2
                        ? username
                        : username.Substring(0, 2) + new string('*', username.Length - 2);
                    return $"{maskedUsername}@{domain}";
                }
            }
            else if (method.Equals("Phone", StringComparison.OrdinalIgnoreCase))
            {
                // Mask phone number: show last 4 digits, hide the rest with ***
                if (destination.Length > 4)
                {
                    return new string('*', destination.Length - 4) + destination.Substring(destination.Length - 4);
                }
            }

            return destination;
        }

        // Determine if the contact info is an email or phone number
        private string DetermineContactType(string contactInfo)
        {
            if (string.IsNullOrEmpty(contactInfo))
                return null;

            // Check if it's an email
            try
            {
                // Simple check if it looks like an email (contains @ and has valid format)
                var mailAddress = new MailAddress(contactInfo);
                return "Email";
            }
            catch
            {
                // Not a valid email, check if it's a phone number
                // This is a very simple check - in production, you'd want more sophisticated validation
                var phonePattern = new Regex(@"^\+?[0-9]{10,15}$");
                if (phonePattern.IsMatch(contactInfo.Trim()))
                {
                    return "Phone";
                }
            }

            // Not recognized as either email or phone
            return null;
        }

        // Mask contact info for logging (privacy protection)
        private string MaskContactInfo(string contactInfo)
        {
            if (string.IsNullOrEmpty(contactInfo))
                return string.Empty;

            var contactType = DetermineContactType(contactInfo);
            if (contactType == "Email")
            {
                return MaskDestination(contactInfo, "Email");
            }
            else if (contactType == "Phone")
            {
                return MaskDestination(contactInfo, "Phone");
            }

            // If can't determine type, just return first and last character
            if (contactInfo.Length <= 2)
                return contactInfo;

            return contactInfo[0] + new string('*', contactInfo.Length - 2) + contactInfo[contactInfo.Length - 1];
        }

        // Clean up expired reset tokens periodically
        private void CleanupExpiredResetTokens()
        {
            var now = DateTime.UtcNow;

            lock (_resetPasswordTokens)
            {
                var expiredTokens = _resetPasswordTokens
                    .Where(pair => pair.Value.ExpiryTime <= now)
                    .Select(pair => pair.Key)
                    .ToList();

                foreach (var token in expiredTokens)
                {
                    _resetPasswordTokens.Remove(token);
                }
            }
        }

        #endregion




    }
}
