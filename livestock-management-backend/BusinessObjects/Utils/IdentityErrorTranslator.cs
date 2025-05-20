using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessObjects.Utils
{
    /// <summary>
    /// Lớp tiện ích để dịch các thông báo lỗi của ASP.NET Identity sang tiếng Việt
    /// </summary>
    public static class IdentityErrorTranslator
    {
        /// <summary>
        /// Dịch danh sách lỗi từ Identity sang tiếng Việt
        /// </summary>
        /// <param name="errors">Danh sách lỗi từ Identity</param>
        /// <returns>Chuỗi lỗi đã dịch sang tiếng Việt</returns>
        public static string TranslateErrors(IEnumerable<IdentityError> errors)
        {
            if (errors == null || !errors.Any())
                return string.Empty;

            var translatedErrors = errors.Select(error => TranslateError(error));
            return string.Join(", ", translatedErrors);
        }

        /// <summary>
        /// Dịch một mã lỗi cụ thể của Identity sang tiếng Việt
        /// </summary>
        /// <param name="error">Lỗi cần dịch</param>
        /// <returns>Chuỗi thông báo lỗi bằng tiếng Việt</returns>
        public static string TranslateError(IdentityError error)
        {
            if (error == null)
                return string.Empty;

            return error.Code switch
            {
                // Lỗi liên quan đến mật khẩu
                "PasswordRequiresDigit" => "Mật khẩu phải chứa ít nhất một chữ số ('0'-'9').",
                "PasswordRequiresLower" => "Mật khẩu phải chứa ít nhất một chữ cái viết thường ('a'-'z').",
                "PasswordRequiresUpper" => "Mật khẩu phải chứa ít nhất một chữ cái viết hoa ('A'-'Z').",
                "PasswordRequiresNonAlphanumeric" => "Mật khẩu phải chứa ít nhất một ký tự đặc biệt.",
                "PasswordTooShort" => "Mật khẩu phải có ít nhất 6 ký tự.",
                "PasswordRequiresUniqueChars" => "Mật khẩu phải có ít nhất 1 ký tự khác nhau.",

                // Lỗi liên quan đến người dùng
                "DuplicateUserName" => "Tên đăng nhập đã tồn tại.",
                "DuplicateEmail" => "Email đã tồn tại.",
                "InvalidUserName" => "Tên đăng nhập không hợp lệ, chỉ có thể chứa chữ cái, số và các ký tự cho phép.",
                "InvalidEmail" => "Email không hợp lệ.",
                "UserAlreadyHasPassword" => "Người dùng đã có mật khẩu.",
                "UserLockoutNotEnabled" => "Tính năng khóa người dùng không được kích hoạt.",

                // Lỗi liên quan đến vai trò
                "InvalidRoleName" => "Tên vai trò không hợp lệ.",
                "DuplicateRoleName" => "Tên vai trò đã tồn tại.",
                "UserAlreadyInRole" => "Người dùng đã có vai trò này.",
                "UserNotInRole" => "Người dùng không có vai trò này.",

                // Lỗi khác
                "LoginAlreadyAssociated" => "Tài khoản đăng nhập này đã được liên kết với tài khoản khác.",
                "PasswordMismatch" => "Mật khẩu không chính xác.",
                "InvalidToken" => "Mã token không hợp lệ.",
                "ConcurrencyFailure" => "Lỗi đồng thời, dữ liệu đã được thay đổi.",

                // Mặc định trả về mô tả gốc
                _ => error.Description
            };
        }
    }
}