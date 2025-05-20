using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.ConfigModels
{
    public class SmsSettings
    {
        public string Provider { get; set; } = "ESMS";
        public TwilioSmsSettings Twilio { get; set; }
        public EsmsSettings Esms { get; set; }
    }

    public class TwilioSmsSettings
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class EsmsSettings
    {
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public int SmsType { get; set; } = 2; // 2 cho OTP/Giao dịch
        public string Brandname { get; set; } = "VERIFY"; // Tên thương hiệu (nếu sử dụng SMS Brandname)
    }
}