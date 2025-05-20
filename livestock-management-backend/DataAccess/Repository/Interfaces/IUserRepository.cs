using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface IUserRepository
    {
        public Task<ResponseModel> Login(LoginVM model);
        Task<ResponseModel> ProcessGoogleLoginAsync(string email, string name, string googleId);
        Task<UserProfileDTO> GetUserProfileAsync(string userId);
        Task<ResponseModel> UpdateUserProfileAsync(string userId, UpdateProfileDTO model);

        // Method to initiate password change and send verification code
        Task<ResponseModel> InitiatePasswordChangeAsync(string userId, InitiatePasswordChangeDTO model);

        // Method to verify code and complete password change
        Task<ResponseModel> CompletePasswordChangeAsync(string userId, CompletePasswordChangeDTO model);

        // Forgot password methods
        Task<ResponseModel> InitiateForgotPasswordAsync(InitiateForgotPasswordDTO model);
        Task<ResponseModel> CompleteForgotPasswordAsync(CompleteForgotPasswordDTO model);
        Task<int> GetUserCountInRole(string roleName);
    }
}
