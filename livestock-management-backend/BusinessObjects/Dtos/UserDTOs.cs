using System;
using System.Collections.Generic;

namespace BusinessObjects.Dtos
{
    public class UserProfileDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
    }

    public class UpdateProfileDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    // DTO for initiating password change and selecting verification method
    public class InitiatePasswordChangeDTO
    {
        public string VerificationMethod { get; set; } // "Email" or "Phone"
    }

    // DTO for completing password change with verification code
    public class CompletePasswordChangeDTO
    {
        public string VerificationCode { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }

    // DTO for initiating the forgot password process
    public class InitiateForgotPasswordDTO
    {
        public string ContactInfo { get; set; } // Can be either email or phone number
    }

    // DTO for completing the forgot password process
    public class CompleteForgotPasswordDTO
    {
        public string ResetToken { get; set; } // Token nhận được từ bước khởi tạo
        public string VerificationCode { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }

    // DTO for role with priority information
    public class RoleWithPriority
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
    }
}
