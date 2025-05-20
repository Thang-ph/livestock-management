using LivestockManagementSystemAPI.Helper.QRCodeGeneratorHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LivestockManagementSystemAPI.Controllers
{
    /// <summary>
    /// API tạo mã QR cho các đối tượng trong hệ thống
    /// </summary>
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Tạo mã QR: tạo mã QR từ nội dung văn bản")]
    public class QRController : BaseAPIController
    {
        private readonly IQRCodeGeneratorHelper qRCodeGeneratorHelper;
        private readonly ILogger<QRController> _logger;


        public QRController(IQRCodeGeneratorHelper qRCodeGeneratorHelper, ILogger<QRController> logger)
        {
            this.qRCodeGeneratorHelper = qRCodeGeneratorHelper;
            _logger = logger;
        }

        /// <summary>
        /// Tạo mã QR từ nội dung văn bản
        /// </summary>
        /// <remarks>
        /// API này nhận một chuỗi văn bản và trả về hình ảnh mã QR dưới dạng chuỗi Base64.
        /// Chuỗi Base64 được trả về có thể được sử dụng trực tiếp trong thẻ img HTML.
        /// 
        /// Ví dụ Request:
        /// ```
        /// GET /api/QR?text=LS001
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "data": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAJYAAACWCAYAAAA8AXHiAAAAAXNSR0IArs4c...",
        ///   "errors": null,
        ///   "message": "Get Success"
        /// }
        /// ```
        /// 
        /// Sử dụng trong HTML:
        /// ```html
        /// <img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAJYAAACWCAYAAAA8AXHiAAAAAXNSR0IArs4c..." alt="QR Code" />
        /// ```
        /// </remarks>
        /// <param name="text">Nội dung văn bản cần mã hóa thành mã QR</param>
        /// <returns>Chuỗi Base64 của hình ảnh mã QR</returns>
        /// <response code="200">Thành công, trả về mã QR dạng Base64</response>
        /// <response code="400">Dữ liệu đầu vào rỗng hoặc không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Index(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Index)} Input empty");
                return GetError("dau vao rong");
            }
            byte[] QRCodeAsBytes = qRCodeGeneratorHelper.GenerateQRCode(text);
            string QRCodeAsImageBase64 = $"data:image/png;base64,{Convert.ToBase64String(QRCodeAsBytes)}";

            return GetSuccess(QRCodeAsImageBase64);
        }
    }
}
