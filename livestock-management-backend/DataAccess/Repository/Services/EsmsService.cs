using BusinessObjects.ConfigModels;
using DataAccess.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;

namespace DataAccess.Repository.Services
{
    public class EsmsService : ISmsService
    {
        private readonly EsmsSettings _settings;
        private readonly ILogger<EsmsService> _logger;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://rest.esms.vn/MainService.svc/json/";

        public EsmsService(
            IOptions<SmsSettings> smsOptions,
            ILogger<EsmsService> logger)
        {
            _settings = smsOptions.Value.Esms ?? throw new ArgumentNullException(nameof(smsOptions), "ESMS settings is missing in configuration");
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public string GetProviderName() => "ESMS";

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {

                if (string.IsNullOrEmpty(_settings.ApiKey) || string.IsNullOrEmpty(_settings.SecretKey))
                {
                    _logger.LogError("ESMS credentials are missing in configuration");
                    return false;
                }

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    _logger.LogError("Phone number is empty");
                    return false;
                }

                // Chuẩn hóa số điện thoại
                phoneNumber = NormalizePhoneNumber(phoneNumber);

                // Cấu trúc dữ liệu theo ví dụ CURL
                var requestData = new
                {
                    ApiKey = _settings.ApiKey,
                    Content = message,
                    Phone = phoneNumber,
                    SecretKey = _settings.SecretKey,
                    Brandname = "Baotrixemay",
                    SmsType = "2",
                    IsUnicode = "0", // 0 = không dấu, 1 = có dấu
                    RequestId = Guid.NewGuid().ToString() // Tạo ID riêng cho mỗi request
                };

                // Thiết lập content với Content-Type đúng
                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Gửi POST request đến endpoint SendMultipleMessage_V4_post_json
                var response = await _httpClient.PostAsync("SendMultipleMessage_V4_post_json/", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("HTTP error when sending SMS via ESMS. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<EsmsResponse>();

                if (result?.CodeResult == "100")
                {
                    _logger.LogInformation("SMS sent successfully via ESMS to {PhoneNumber}, SMSID: {SmsId}", phoneNumber, result.SMSID);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send SMS via ESMS. Error code: {CodeResult}, Message: {ErrorMessage}",
                        result?.CodeResult, result?.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending SMS via ESMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            // Bỏ dấu + nếu có
            phoneNumber = phoneNumber.Trim().TrimStart('+');

            // Nếu số bắt đầu bằng 84, giữ nguyên
            if (phoneNumber.StartsWith("84"))
                return phoneNumber;

            // Nếu số bắt đầu bằng 0, thay bằng 84
            if (phoneNumber.StartsWith("0"))
                return "84" + phoneNumber.Substring(1);

            // Trường hợp còn lại, thêm 84 vào đầu nếu là số Việt Nam
            return "84" + phoneNumber;
        }
    }

    public class EsmsResponse
    {
        public string CodeResult { get; set; }
        public string ErrorMessage { get; set; }
        public string SMSID { get; set; }
        public int CountRegenerate { get; set; }
    }
}