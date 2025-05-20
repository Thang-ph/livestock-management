using BusinessObjects.ConfigModels;
using BusinessObjects.Utils;
using DataAccess.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DataAccess.Repository.Services
{
    /// <summary>
    /// Dịch vụ gửi email
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        /// <summary>
        /// Khởi tạo dịch vụ gửi email
        /// </summary>
        /// <param name="emailSettings">Cấu hình email</param>
        /// <param name="logger">Logger</param>
        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gửi mã xác thực đến địa chỉ email
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="code">Mã xác thực</param>
        /// <returns>Kết quả gửi email</returns>
        public async Task<bool> SendVerificationCodeAsync(string email, string code)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Sending verification code to: {Email}",
                nameof(EmailService), nameof(SendVerificationCodeAsync), email);

            var subject = "Mã xác nhận từ hợp tác xã Lúa Vàng";
            var htmlBody = EmailTemplates.VerificationCode(code);

            return await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Gửi thông báo chung đến địa chỉ email
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="content">Nội dung thông báo</param>
        /// <returns>Kết quả gửi email</returns>
        public async Task<bool> SendNotificationAsync(string email, string subject, string content)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Sending notification to: {Email}, Subject: {Subject}",
                nameof(EmailService), nameof(SendNotificationAsync), email, subject);

            var htmlBody = EmailTemplates.GeneralNotification(subject, content);

            return await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Gửi thông báo về lô tiêm
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="batchInfo">Thông tin lô tiêm</param>
        /// <returns>Kết quả gửi email</returns>
        public async Task<bool> SendVaccinationBatchNotificationAsync(string email, object batchInfo)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Sending vaccination batch notification to: {Email}",
                nameof(EmailService), nameof(SendVaccinationBatchNotificationAsync), email);

            var subject = "Thông báo lô tiêm từ HTX Lúa Vàng";

            // Chuyển đổi object thành Dictionary
            var batchInfoDict = ConvertObjectToDictionary(batchInfo);
            var htmlBody = EmailTemplates.VaccinationBatchNotification(batchInfoDict);

            return await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Gửi thông báo về dịch bệnh
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="diseaseInfo">Thông tin dịch bệnh</param>
        /// <returns>Kết quả gửi email</returns>
        public async Task<bool> SendDiseaseAlertAsync(string email, object diseaseInfo)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Sending disease alert to: {Email}",
                nameof(EmailService), nameof(SendDiseaseAlertAsync), email);

            var subject = "CẢNH BÁO: Thông tin dịch bệnh từ HTX Lúa Vàng";

            // Chuyển đổi object thành Dictionary
            var diseaseInfoDict = ConvertObjectToDictionary(diseaseInfo);
            var htmlBody = EmailTemplates.DiseaseAlert(diseaseInfoDict);

            return await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Gửi thông báo về gói thầu
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="procurementInfo">Thông tin gói thầu</param>
        /// <returns>Kết quả gửi email</returns>
        public async Task<bool> SendProcurementNotificationAsync(string email, object procurementInfo)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Sending procurement notification to: {Email}",
                nameof(EmailService), nameof(SendProcurementNotificationAsync), email);

            var subject = "Thông báo gói thầu từ HTX Lúa Vàng";

            // Chuyển đổi object thành Dictionary
            var procurementInfoDict = ConvertObjectToDictionary(procurementInfo);
            var htmlBody = EmailTemplates.ProcurementNotification(procurementInfoDict);

            return await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Gửi email đặt lại mật khẩu
        /// </summary>
        /// <param name="email">Địa chỉ email nhận</param>
        /// <param name="resetLink">Liên kết đặt lại mật khẩu</param>
        /// <returns>Kết quả gửi email</returns>
        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink)
        {
            _logger.LogInformation("[{ServiceName}.{MethodName}] - Sending password reset email to: {Email}",
                nameof(EmailService), nameof(SendPasswordResetEmailAsync), email);

            var subject = "Đặt lại mật khẩu cho tài khoản HTX Lúa Vàng";

            // Xây dựng email dạng HTML đơn giản cho đặt lại mật khẩu
            var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ padding: 20px; }}
                    .button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Đặt Lại Mật Khẩu</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào,</p>
                        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại HTX Lúa Vàng. Vui lòng nhấp vào liên kết dưới đây để đặt lại mật khẩu của bạn:</p>
                        <p><a href='{resetLink}' class='button'>Đặt lại mật khẩu</a></p>
                        <p>Hoặc sao chép liên kết này vào trình duyệt của bạn: <br>{resetLink}</p>
                        <p>Liên kết này có hiệu lực trong vòng 30 phút.</p>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <p>Trân trọng,<br>HTX Lúa Vàng</p>
                    </div>
                    <div class='footer'>
                        <p>Đây là email tự động, vui lòng không trả lời.</p>
                    </div>
                </div>
            </body>
            </html>";

            return await SendEmailAsync(email, subject, htmlBody);
        }

        /// <summary>
        /// Gửi email với nội dung HTML
        /// </summary>
        /// <param name="toEmail">Địa chỉ email nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="htmlBody">Nội dung HTML</param>
        /// <returns>Kết quả gửi email</returns>
        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                if (string.IsNullOrEmpty(toEmail))
                {
                    _logger.LogWarning("[{ServiceName}.{MethodName}] - Email recipient is null or empty",
                        nameof(EmailService), nameof(SendEmailAsync));
                    return false;
                }

                var fromAddress = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName ?? "Hợp tác xã Lúa Vàng");
                var toAddress = new MailAddress(toEmail);

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                // IMPORTANT: UseDefaultCredentials MUST be set before setting Credentials
                using var smtp = new SmtpClient(_emailSettings.SmtpServer)
                {
                    UseDefaultCredentials = false,
                    Port = _emailSettings.Port,
                    EnableSsl = _emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                if (_emailSettings.EnableDetailedLogging)
                {
                    _logger.LogDebug("[{ServiceName}.{MethodName}] - SMTP Settings: Server={Server}, Port={Port}, SSL={SSL}, Username={Username}",
                        nameof(EmailService), nameof(SendEmailAsync), _emailSettings.SmtpServer, _emailSettings.Port,
                        _emailSettings.EnableSsl, _emailSettings.Username);
                }

                _logger.LogInformation("[{ServiceName}.{MethodName}] - Attempting to send email to {Email}",
                    nameof(EmailService), nameof(SendEmailAsync), toEmail);

                await smtp.SendMailAsync(message);

                _logger.LogInformation("[{ServiceName}.{MethodName}] - Email sent successfully to {Email}",
                    nameof(EmailService), nameof(SendEmailAsync), toEmail);

                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "[{ServiceName}.{MethodName}] - SMTP error sending email: {ErrorMessage}",
                    nameof(EmailService), nameof(SendEmailAsync), smtpEx.Message);

                if (smtpEx.StatusCode != 0)
                {
                    _logger.LogError("[{ServiceName}.{MethodName}] - SMTP Status Code: {StatusCode}",
                        nameof(EmailService), nameof(SendEmailAsync), smtpEx.StatusCode);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ServiceName}.{MethodName}] - Error sending email: {ErrorMessage}",
                    nameof(EmailService), nameof(SendEmailAsync), ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Chuyển đổi object thành Dictionary<string, string>
        /// </summary>
        /// <param name="obj">Object cần chuyển đổi</param>
        /// <returns>Dictionary kết quả</returns>
        private Dictionary<string, string> ConvertObjectToDictionary(object obj)
        {
            var result = new Dictionary<string, string>();

            if (obj == null)
                return result;

            // Nếu obj đã là Dictionary<string, string>
            if (obj is Dictionary<string, string> dictStringString)
                return dictStringString;

            // Nếu obj là từ điển khác loại, cố gắng chuyển đổi
            if (obj is IDictionary<string, object> dictObj)
            {
                foreach (var pair in dictObj)
                {
                    result[pair.Key] = pair.Value?.ToString() ?? string.Empty;
                }
                return result;
            }

            // Sử dụng reflection để lấy các thuộc tính
            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                result[prop.Name] = value?.ToString() ?? string.Empty;
            }

            return result;
        }
    }
}