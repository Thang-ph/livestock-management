namespace BusinessObjects.ConfigModels
{
    /// <summary>
    /// Cấu hình cho dịch vụ gửi email
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// Tên người gửi hiển thị
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Địa chỉ email người gửi
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Máy chủ SMTP
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// Cổng SMTP
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Tên người dùng để đăng nhập vào máy chủ SMTP
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Mật khẩu để đăng nhập vào máy chủ SMTP
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Cho phép SSL khi gửi email
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Cho phép logging chi tiết khi gửi email
        /// </summary>
        public bool EnableDetailedLogging { get; set; }
    }
}