using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi mã xác thực đến địa chỉ email
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="code">Mã xác thực</param>
        /// <returns>Kết quả gửi email</returns>
        Task<bool> SendVerificationCodeAsync(string email, string code);

        /// <summary>
        /// Gửi thông báo chung đến địa chỉ email
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="content">Nội dung thông báo</param>
        /// <returns>Kết quả gửi email</returns>
        Task<bool> SendNotificationAsync(string email, string subject, string content);

        /// <summary>
        /// Gửi thông báo về lô tiêm
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="batchInfo">Thông tin lô tiêm</param>
        /// <returns>Kết quả gửi email</returns>
        Task<bool> SendVaccinationBatchNotificationAsync(string email, object batchInfo);

        /// <summary>
        /// Gửi thông báo về dịch bệnh
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="diseaseInfo">Thông tin dịch bệnh</param>
        /// <returns>Kết quả gửi email</returns>
        Task<bool> SendDiseaseAlertAsync(string email, object diseaseInfo);

        /// <summary>
        /// Gửi thông báo về gói thầu
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="procurementInfo">Thông tin gói thầu</param>
        /// <returns>Kết quả gửi email</returns>
        Task<bool> SendProcurementNotificationAsync(string email, object procurementInfo);

        /// <summary>
        /// Gửi email đặt lại mật khẩu
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="resetLink">Liên kết đặt lại mật khẩu</param>
        /// <returns>Kết quả gửi email</returns>
        Task<bool> SendPasswordResetEmailAsync(string email, string resetLink);
    }
}