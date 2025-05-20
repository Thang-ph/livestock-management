using BusinessObjects.ConfigModels;
using DataAccess.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace DataAccess.Repository.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly SmsSettings _smsSettings;
        private readonly ILogger<TwilioSmsService> _logger;

        public TwilioSmsService(
            IOptions<SmsSettings> smsOptions,
            ILogger<TwilioSmsService> logger)
        {
            _smsSettings = smsOptions.Value;
            _logger = logger;
        }

        public string GetProviderName() => "Twilio";

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                _logger.LogInformation("Attempting to send SMS via Twilio to {PhoneNumber}", phoneNumber);

                // Kiểm tra cấu hình
                if (_smsSettings.Twilio == null ||
                    string.IsNullOrEmpty(_smsSettings.Twilio.AccountSid) ||
                    string.IsNullOrEmpty(_smsSettings.Twilio.AuthToken) ||
                    string.IsNullOrEmpty(_smsSettings.Twilio.PhoneNumber))
                {
                    _logger.LogError("Twilio configuration is missing. Please check SmsSettings in appsettings.json");
                    return false;
                }

                // Chuẩn hóa số điện thoại
                if (!phoneNumber.StartsWith("+"))
                {
                    if (phoneNumber.StartsWith("0"))
                    {
                        phoneNumber = "+84" + phoneNumber.Substring(1);
                    }
                    else
                    {
                        phoneNumber = "+84" + phoneNumber;
                    }
                }

                // Giả lập gửi SMS via Twilio
                _logger.LogInformation("SMS would be sent via Twilio from {FromNumber} to {ToNumber} with message: {Message}",
                    _smsSettings.Twilio.PhoneNumber, phoneNumber, message);

                // Trong thực tế, bạn sẽ gọi Twilio API ở đây:
                /*
                TwilioClient.Init(_smsSettings.Twilio.AccountSid, _smsSettings.Twilio.AuthToken);
                var messageOptions = new CreateMessageOptions(new PhoneNumber(phoneNumber))
                {
                    From = new PhoneNumber(_smsSettings.Twilio.PhoneNumber),
                    Body = message
                };
                var messageResponse = await MessageResource.CreateAsync(messageOptions);
                return messageResponse.Status == MessageResource.StatusEnum.Queued ||
                       messageResponse.Status == MessageResource.StatusEnum.Sent;
                */

                // Giả lập thành công
                _logger.LogInformation("SMS sent successfully via Twilio to {PhoneNumber}", phoneNumber);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending SMS via Twilio to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
}