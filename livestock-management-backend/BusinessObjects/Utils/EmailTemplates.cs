using System.Collections.Generic;

namespace BusinessObjects.Utils
{
    /// <summary>
    /// Định nghĩa các mẫu email được sử dụng trong hệ thống
    /// </summary>
    public static class EmailTemplates
    {
        /// <summary>
        /// Tạo mẫu email chứa mã xác nhận
        /// </summary>
        /// <param name="code">Mã xác nhận</param>
        /// <returns>Nội dung HTML của email</returns>
        public static string VerificationCode(string code)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ padding: 20px; }}
                    .code {{ font-size: 32px; font-weight: bold; text-align: center; margin: 20px 0; letter-spacing: 5px; color: #4CAF50; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Mã Xác Nhận</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào,</p>
                        <p>Bạn đã yêu cầu mã xác nhận từ hệ thống quản lý chăn nuôi HTX Lúa Vàng. Vui lòng sử dụng mã dưới đây để hoàn tất quá trình xác thực:</p>
                        <div class='code'>{code}</div>
                        <p>Mã này có hiệu lực trong vòng 5 phút.</p>
                        <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                        <p>Trân trọng,<br>HTX Lúa Vàng</p>
                    </div>
                    <div class='footer'>
                        <p>Đây là email tự động, vui lòng không trả lời.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// Tạo mẫu email thông báo chung
        /// </summary>
        /// <param name="subject">Tiêu đề thông báo</param>
        /// <param name="content">Nội dung thông báo</param>
        /// <returns>Nội dung HTML của email</returns>
        public static string GeneralNotification(string subject, string content)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ padding: 20px; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>{subject}</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào,</p>
                        <div>{content}</div>
                        <p>Trân trọng,<br>HTX Lúa Vàng</p>
                    </div>
                    <div class='footer'>
                        <p>Đây là email tự động từ hệ thống quản lý chăn nuôi HTX Lúa Vàng.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// Tạo mẫu email thông báo về lô tiêm
        /// </summary>
        /// <param name="batchInfo">Thông tin về lô tiêm</param>
        /// <returns>Nội dung HTML của email</returns>
        public static string VaccinationBatchNotification(Dictionary<string, string> batchInfo)
        {
            var batchDetailsHtml = "";
            foreach (var item in batchInfo)
            {
                batchDetailsHtml += $@"<tr><th style='text-align: left; padding: 8px; border-bottom: 1px solid #ddd;'>{item.Key}</th><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{item.Value}</td></tr>";
            }

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ padding: 20px; }}
                    table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                    th, td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
                    th {{ background-color: #f2f2f2; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Thông Báo Lô Tiêm</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào,</p>
                        <p>HTX Lúa Vàng xin thông báo thông tin về lô tiêm:</p>
                        <table>
                            {batchDetailsHtml}
                        </table>
                        <p>Vui lòng kiểm tra thông tin và thực hiện các biện pháp cần thiết.</p>
                        <p>Trân trọng,<br>HTX Lúa Vàng</p>
                    </div>
                    <div class='footer'>
                        <p>Đây là email tự động từ hệ thống quản lý chăn nuôi HTX Lúa Vàng.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// Tạo mẫu email cảnh báo dịch bệnh
        /// </summary>
        /// <param name="diseaseInfo">Thông tin về dịch bệnh</param>
        /// <returns>Nội dung HTML của email</returns>
        public static string DiseaseAlert(Dictionary<string, string> diseaseInfo)
        {
            var diseaseDetailsHtml = "";
            foreach (var item in diseaseInfo)
            {
                diseaseDetailsHtml += $@"<tr><th style='text-align: left; padding: 8px; border-bottom: 1px solid #ddd;'>{item.Key}</th><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{item.Value}</td></tr>";
            }

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                    .header {{ background-color: #F44336; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ padding: 20px; }}
                    table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                    th, td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
                    th {{ background-color: #f2f2f2; }}
                    .warning {{ background-color: #FFEBEE; border-left: 4px solid #F44336; padding: 10px; margin: 10px 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Cảnh Báo Dịch Bệnh</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào,</p>
                        <div class='warning'>
                            <p><strong>CẢNH BÁO:</strong> HTX Lúa Vàng xin thông báo về tình hình dịch bệnh đã được phát hiện:</p>
                        </div>
                        <table>
                            {diseaseDetailsHtml}
                        </table>
                        <p>Vui lòng thực hiện các biện pháp phòng ngừa và tuân thủ hướng dẫn từ HTX.</p>
                        <p>Trân trọng,<br>HTX Lúa Vàng</p>
                    </div>
                    <div class='footer'>
                        <p>Đây là email tự động từ hệ thống quản lý chăn nuôi HTX Lúa Vàng.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// Tạo mẫu email thông báo về gói thầu
        /// </summary>
        /// <param name="procurementInfo">Thông tin về gói thầu</param>
        /// <returns>Nội dung HTML của email</returns>
        public static string ProcurementNotification(Dictionary<string, string> procurementInfo)
        {
            var procurementDetailsHtml = "";
            foreach (var item in procurementInfo)
            {
                procurementDetailsHtml += $@"<tr><th style='text-align: left; padding: 8px; border-bottom: 1px solid #ddd;'>{item.Key}</th><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{item.Value}</td></tr>";
            }

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                    .header {{ background-color: #2196F3; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ padding: 20px; }}
                    table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                    th, td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
                    th {{ background-color: #f2f2f2; }}
                    .highlight {{ background-color: #E3F2FD; border-left: 4px solid #2196F3; padding: 10px; margin: 10px 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Thông Báo Gói Thầu</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào,</p>
                        <div class='highlight'>
                            <p>HTX Lúa Vàng xin thông báo về gói thầu sau:</p>
                        </div>
                        <table>
                            {procurementDetailsHtml}
                        </table>
                        <p>Để biết thêm chi tiết, vui lòng liên hệ với HTX Lúa Vàng.</p>
                        <p>Trân trọng,<br>HTX Lúa Vàng</p>
                    </div>
                    <div class='footer'>
                        <p>Đây là email tự động từ hệ thống quản lý chăn nuôi HTX Lúa Vàng.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}