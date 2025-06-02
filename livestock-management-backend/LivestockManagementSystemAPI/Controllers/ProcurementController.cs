using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using static BusinessObjects.Constants.LmsConstants;
//using ClosedXML.Excel;
using DataAccess.Repository.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO.Packaging;
using OfficeOpenXml;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/procurement-management")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý gói thầu: tạo, cập nhật, và theo dõi các gói thầu và quá trình xuất gia súc")]
    public class ProcurementController : BaseAPIController
    {
        private readonly IProcurementRepository _procurementRepository;
        private readonly ICloudinaryRepository _cloudinaryRepository;
        private readonly ILogger<ProcurementController> _logger;


        public ProcurementController(IProcurementRepository procurementRepository,
            ICloudinaryRepository cloudinaryRepository,
            ILogger<ProcurementController> logger)
        {
            _procurementRepository = procurementRepository;
            _cloudinaryRepository = cloudinaryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của gói thầu
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin chi tiết của một gói thầu dựa trên ID, bao gồm thông tin cơ bản và danh sách các yêu cầu kỹ thuật.
        /// 
        /// Thông tin trả về bao gồm:
        /// - Thông tin cơ bản về gói thầu (ID, tên, ngày tạo, trạng thái...)
        /// - Danh sách các yêu cầu kỹ thuật cho gói thầu
        /// - Các thông số và điều kiện thầu
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Lấy thông tin gói thầu thành công",
        ///   "data": {
        ///     "id": "64f98c2d7b87af0c1839b7a2", // |ID của gói thầu|
        ///     "code": "PROC2023Q3", // |Mã gói thầu|
        ///     "name": "Gói thầu gia súc Q3/2023", // |Tên gói thầu|
        ///     "status": "ĐANG_ĐẤU_THẦU", // |Trạng thái gói thầu|
        ///     "successDate": null, // |Ngày thành công|
        ///     "expirationDate": "2023-09-30T00:00:00", // |Ngày hết hạn|
        ///     "completionDate": null, // |Ngày hoàn thành|
        ///     "totalExported": 0, // |Tổng số đã xuất|
        ///     "totalRequired": 0, // |Tổng số yêu cầu|
        ///     "createdAt": "2023-06-15T10:30:45", // |Ngày tạo|
        ///     "owner": "Công ty Chăn nuôi XYZ", // |Tên bên mời thầu|
        ///     "expiredDuration": 90, // |Thời hạn gói thầu (ngày)|
        ///     "description": "Gói thầu bò, lợn cho quý 3 năm 2023", // |Mô tả chi tiết|
        ///     "createdBy": "admin", // |Người tạo gói thầu|
        ///     "details": [ // |Danh sách yêu cầu kỹ thuật|
        ///       {
        ///         "id": "64f98c2d7b87af0c1839b7a3", // |ID của yêu cầu|
        ///         "procurementName": "Gói thầu gia súc Q3/2023", // |Tên gói thầu|
        ///         "speciesId": "S001", // |ID của loài gia súc|
        ///         "speciesName": "Bò sữa", // |Tên loài gia súc|
        ///         "requiredWeightMin": 200, // |Cân nặng tối thiểu (kg)|
        ///         "requiredWeightMax": 300, // |Cân nặng tối đa (kg)|
        ///         "requiredAgeMin": 12, // |Tuổi tối thiểu (tháng)|
        ///         "requiredAgeMax": 24, // |Tuổi tối đa (tháng)|
        ///         "description": "Gia súc phải khỏe mạnh và có giấy kiểm dịch", // |Mô tả yêu cầu|
        ///         "requiredQuantity": 50, // |Số lượng cần mua|
        ///         "requiredInsurance": 30 // |Thời hạn bảo hành (ngày)|
        ///       }
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của gói thầu cần lấy thông tin</param>
        /// <returns>Thông tin chi tiết của gói thầu</returns>
        /// <response code="200">Thành công, trả về thông tin chi tiết gói thầu</response>
        /// <response code="404">Không tìm thấy gói thầu với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("general-info/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProcurementGeneralInfo>> GetProcurementGeneralInfo([FromRoute] string id)
        {
            try
            {
                var data = await _procurementRepository.GetProcurementGeneralInfo(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetProcurementGeneralInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("get-process-handover-procurement-list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ProcurementGeneralInfo>>> GetProcessHandOverProcurementList()
        {
            try
            {
                var data = await _procurementRepository.GetProcessHandOverProcurementList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetProcessHandOverProcurementList)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("get-procurement-overview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProcurementOverview>> GetPrucrementPreview()
        {
            try
            {
                var data = await _procurementRepository.GetPrucrementPreview();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetPrucrementPreview)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        /// <summary>
        /// Tạo một gói thầu mới
        /// </summary>
        /// <remarks>
        /// API này tạo một gói thầu mới với thông tin cơ bản và các yêu cầu kỹ thuật được cung cấp.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "code": "PROC2023Q3", // |Mã gói thầu|
        ///   "name": "Gói thầu gia súc Q3/2023", // |Tên gói thầu|
        ///   "owner": "Công ty Chăn nuôi XYZ", // |Tên bên mời thầu|
        ///   "expiredDuration": 90, // |Thời hạn gói thầu (ngày)|
        ///   "description": "Gói thầu bò, lợn cho quý 3 năm 2023", // |Mô tả chi tiết về gói thầu|
        ///   "details": [ // |Danh sách các yêu cầu kỹ thuật|
        ///     {
        ///       "speciesId": "S001", // |ID của loài gia súc|
        ///       "requiredQuantity": 50, // |Số lượng cần mua|
        ///       "requiredWeightMin": 200, // |Cân nặng tối thiểu (kg)|
        ///       "requiredWeightMax": 300, // |Cân nặng tối đa (kg)|
        ///       "requiredAgeMin": 12, // |Tuổi tối thiểu (tháng)|
        ///       "requiredAgeMax": 24, // |Tuổi tối đa (tháng)|
        ///       "requiredInsuranceDuration": 30, // |Thời hạn bảo hành (ngày)|
        ///       "description": "Gia súc phải khỏe mạnh và có giấy kiểm dịch" // |Mô tả yêu cầu|
        ///     }
        ///   ],
        ///   "requestedBy": "Admin123" // |ID của người thực hiện hành động|
        /// }
        /// ```
        /// 
        /// Response Body:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Tạo gói thầu thành công",
        ///   "data": {
        ///     "id": "64f98c2d7b87af0c1839b7a2", // |ID của gói thầu vừa tạo|
        ///     "code": "PROC2023Q3", // |Mã gói thầu|
        ///     "name": "Gói thầu gia súc Q3/2023", // |Tên gói thầu|
        ///     "status": "ĐANG_ĐẤU_THẦU", // |Trạng thái gói thầu mới|
        ///     "successDate": null, // |Ngày thành công|
        ///     "expirationDate": "2023-09-30T00:00:00", // |Ngày hết hạn|
        ///     "completionDate": null, // |Ngày hoàn thành|
        ///     "totalExported": 0, // |Tổng số đã xuất|
        ///     "totalRequired": 0, // |Tổng số yêu cầu|
        ///     "createdAt": "2023-06-15T10:30:45" // |Ngày tạo gói thầu|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Thông tin gói thầu cần tạo</param>
        /// <returns>Thông tin tóm tắt gói thầu đã tạo</returns>
        /// <response code="200">Thành công, trả về thông tin tóm tắt gói thầu đã tạo</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProcurementSummary>> CreateProcurementPackage([FromBody] CreateProcurementPackageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"[{this.GetType().Name}]/{nameof(CreateProcurementPackage)} ModelState not Valid");
                    throw new Exception(string.Join("; ", ModelState.Values
                                              .SelectMany(x => x.Errors)
                                              .Select(x => x.ErrorMessage)));
                }
                request.RequestedBy = UserId;
                var data = await _procurementRepository.CreateProcurementPackage(request);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CreateProcurementPackage)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin gói thầu
        /// </summary>
        /// <remarks>
        /// API này cập nhật thông tin của gói thầu hiện có, bao gồm thông tin cơ bản và các yêu cầu kỹ thuật.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "id": "64f98c2d7b87af0c1839b7a2", // |ID của gói thầu cần cập nhật|
        ///   "code": "PROC2023Q3-UPDATE", // |Mã gói thầu mới|
        ///   "name": "Gói thầu gia súc Q3/2023 - Đã cập nhật", // |Tên gói thầu mới|
        ///   "owner": "Công ty Chăn nuôi XYZ", // |Tên bên mời thầu|
        ///   "expiredDuration": 120, // |Thời hạn gói thầu mới (ngày)|
        ///   "description": "Gói thầu bò, lợn cho quý 3 năm 2023 - Đã điều chỉnh", // |Mô tả chi tiết mới|
        ///   "details": [ // |Danh sách các yêu cầu kỹ thuật|
        ///     {
        ///       "id": "64f98c2d7b87af0c1839b7a3", // |ID của yêu cầu cần cập nhật|
        ///       "speciesId": "S001", // |ID của loài gia súc|
        ///       "requiredQuantity": 75, // |Số lượng mới|
        ///       "requiredWeightMin": 220, // |Cân nặng tối thiểu mới (kg)|
        ///       "requiredWeightMax": 320, // |Cân nặng tối đa mới (kg)|
        ///       "requiredAgeMin": 15, // |Tuổi tối thiểu mới (tháng)|
        ///       "requiredAgeMax": 30, // |Tuổi tối đa mới (tháng)|
        ///       "requiredInsurance": 45, // |Thời hạn bảo hành mới (ngày)|
        ///       "description": "Gia súc phải khỏe mạnh, có giấy kiểm dịch và nguồn gốc rõ ràng" // |Mô tả yêu cầu mới|
        ///     }
        ///   ]
        /// }
        /// ```
        /// 
        /// Response Body:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Cập nhật gói thầu thành công",
        ///   "data": {
        ///     "id": "64f98c2d7b87af0c1839b7a2", // |ID của gói thầu|
        ///     "code": "PROC2023Q3-UPDATE", // |Mã gói thầu đã cập nhật|
        ///     "name": "Gói thầu gia súc Q3/2023 - Đã cập nhật", // |Tên gói thầu đã cập nhật|
        ///     "status": "ĐANG_ĐẤU_THẦU", // |Trạng thái gói thầu|
        ///     "successDate": null, // |Ngày thành công|
        ///     "expirationDate": "2023-10-30T00:00:00", // |Ngày hết hạn mới|
        ///     "completionDate": null, // |Ngày hoàn thành|
        ///     "totalExported": 0, // |Tổng số đã xuất|
        ///     "totalRequired": 0, // |Tổng số yêu cầu|
        ///     "createdAt": "2023-06-15T10:30:45" // |Ngày tạo gói thầu|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Thông tin gói thầu cần cập nhật</param>
        /// <returns>Thông tin tóm tắt gói thầu đã cập nhật</returns>
        /// <response code="200">Thành công, trả về thông tin tóm tắt gói thầu đã cập nhật</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="404">Không tìm thấy gói thầu với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProcurementSummary>> UpdateProcurementPackage([FromBody] UpdateProcurementPackageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(UpdateProcurementPackage)} ModelState not Valid");
                    throw new Exception(string.Join("; ", ModelState.Values
                                              .SelectMany(x => x.Errors)
                                              .Select(x => x.ErrorMessage)));
                }
                request.RequestedBy = UserId;
                var data = await _procurementRepository.UpdateProcurementPackage(request);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateProcurementPackage)} " + ex.Message);
                return SaveError(ex.Message);

            }
        }

        /// <summary>
        /// Lấy danh sách gói thầu theo bộ lọc
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách gói thầu theo các điều kiện lọc được cung cấp.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "keyword": "bò", // |Từ khóa tìm kiếm trong mã hoặc tên gói thầu|
        ///   "status": "ĐANG_ĐẤU_THẦU", // |Trạng thái gói thầu cần lọc|
        ///   "fromDate": "2023-01-01T00:00:00", // |Ngày bắt đầu lọc|
        ///   "toDate": "2023-12-31T23:59:59", // |Ngày kết thúc lọc|
        ///   "skip": 0, // |Số bản ghi cần bỏ qua|
        ///   "take": 10 // |Số bản ghi cần lấy|
        /// }
        /// ```
        /// 
        /// Response Body:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Lấy danh sách gói thầu thành công",
        ///   "data": {
        ///     "total": 15, // |Tổng số gói thầu thỏa mãn điều kiện|
        ///     "items": [
        ///       {
        ///         "id": "64f98c2d7b87af0c1839b7a2", // |ID của gói thầu|
        ///         "code": "PROC2023Q1", // |Mã gói thầu|
        ///         "name": "Gói thầu bò sữa Q1/2023", // |Tên gói thầu|
        ///         "status": "ĐANG_ĐẤU_THẦU", // |Trạng thái gói thầu|
        ///         "successDate": null, // |Ngày thành công|
        ///         "expirationDate": "2023-03-31T23:59:59", // |Ngày hết hạn|
        ///         "completionDate": null, // |Ngày hoàn thành|
        ///         "totalExported": 0, // |Tổng số đã xuất|
        ///         "totalRequired": 120, // |Tổng số yêu cầu|
        ///         "createdAt": "2023-01-10T09:30:45" // |Ngày tạo|
        ///       }
        ///       // ... Các gói thầu khác
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="filter">Bộ lọc cho danh sách gói thầu</param>
        /// <returns>Danh sách gói thầu phân trang theo bộ lọc</returns>
        /// <response code="200">Thành công, trả về danh sách gói thầu</response>
        /// <response code="400">Dữ liệu lọc không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListProcurements>> GetListProcurements([FromQuery] ListProcurementsFilter filter)
        {
            try
            {
                var data = await _procurementRepository.GetListProcurements(filter);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListProcurements)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Hủy gói thầu
        /// </summary>
        /// <remarks>
        /// API này hủy một gói thầu và cập nhật trạng thái của nó thành "ĐÃ HỦY".
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "id": "64f98c2d7b87af0c1839b7a2", // |ID của gói thầu cần hủy|
        ///   "requestedBy": "admin" // |ID của người thực hiện hành động|
        /// }
        /// ```
        /// 
        /// Response Body:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Hủy gói thầu thành công",
        ///   "data": true
        /// }
        /// ```
        /// </remarks>
        /// <param name="status">Thông tin hủy gói thầu</param>
        /// <returns>Kết quả thay đổi trạng thái</returns>
        /// <response code="200">Thành công, trả về kết quả thay đổi trạng thái</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="404">Không tìm thấy gói thầu với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("cancel-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CancelProcurementPackage([FromBody] ProcurementStatus status)
        {
            try
            {
                var changeProcurementStatusModel = await _procurementRepository.CancelProcurementPackage(status);
                return SaveSuccess(changeProcurementStatusModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CancelProcurementPackage)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPost("accept-procurment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> AcceptProcurementPackage([FromBody] ProcurementStatus status)
        {
            try
            {
                var changeProcurementStatusModel = await _procurementRepository.AcceptProcurementPackage(status);
                return SaveSuccess(changeProcurementStatusModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CancelProcurementPackage)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Hoàn thành gói thầu
        /// </summary>
        /// <remarks>
        /// API này đánh dấu một gói thầu là hoàn thành và cập nhật trạng thái của nó thành "HOÀN THÀNH".
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "id": "64f98c2d7b87af0c1839b7a2", // |ID của gói thầu cần đánh dấu hoàn thành|
        ///   "requestedBy": "admin" // |ID của người thực hiện hành động|
        /// }
        /// ```
        /// 
        /// Response Body:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Hoàn thành gói thầu thành công",
        ///   "data": true
        /// }
        /// ```
        /// </remarks>
        /// <param name="status">Thông tin hoàn thành gói thầu</param>
        /// <returns>Kết quả thay đổi trạng thái</returns>
        /// <response code="200">Thành công, trả về kết quả thay đổi trạng thái</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="404">Không tìm thấy gói thầu với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("complete-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CompleteProcurementPackage([FromBody] ProcurementStatus status)
        {
            try
            {
                var changeProcurementStatusModel = await _procurementRepository.CompleteProcurementPackage(status);
                return SaveSuccess(changeProcurementStatusModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CompleteProcurementPackage)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Xuất danh sách gia súc phù hợp với gói thầu ra file Excel
        /// </summary>
        /// <remarks>
        /// API này tạo và trả về file Excel chứa danh sách gia súc phù hợp với yêu cầu kỹ thuật của gói thầu.
        /// File Excel này sẽ chứa thông tin chi tiết về các gia súc theo tiêu chí của gói thầu, như loài, tuổi, cân nặng...
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// "PROC001"  -> |ID của gói thầu cần xuất danh sách gia súc|
        /// ```
        /// </remarks>
        /// <param name="procurementId">ID của gói thầu cần xuất danh sách gia súc</param>
        /// <returns>File Excel chứa danh sách gia súc phù hợp</returns>
        /// <response code="200">Thành công, trả về file Excel</response>
        /// <response code="400">Không tìm thấy dữ liệu phù hợp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("export-suggest-excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportExcel([FromBody] string procurementId)
        {
            try
            {
                var filteredData = await _procurementRepository.GetEmpData(procurementId);

                if (filteredData.Rows.Count == 0)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(ExportExcel)} No matching data found.");
                    return GetError("Không tìm thấy vật nuôi phù hợp");
                }

                //var folderName = "livestocks-suggestion";
                var fileName = $"Danh sách vật nuôi đề xuất_{DateTime.Now.ToString("yyyyMMddhhmmss")}.xlsx";
                //string tempFilePath = Path.Combine(Path.GetTempPath(), $"{filename}x");
                //using (XLWorkbook wb = new XLWorkbook())
                //{

                //    var ws = wb.AddWorksheet(filteredData, "Vật nuôi đề xuất");
                //    ws.Columns().AdjustToContents();
                //    wb.SaveAs(tempFilePath);

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(new MemoryStream());
                    var worksheet = package.Workbook.Worksheets.Add($"Danh sách vật nuôi đề xuất");
                    worksheet.Cells["A1"].LoadFromDataTable(filteredData, true);
                    await package.SaveAsync();
                    var stream = package.Stream;
                    stream.Position = 0;
                    var url = await _cloudinaryRepository.UploadFileStreamAsync(CloudFolderFileTemplateName, fileName, stream);
                    return GetSuccess(new { Url = url });

                    //using (MemoryStream ms = new MemoryStream())
                    //{
                    //    wb.SaveAs(ms);
                    //    return File(ms.ToArray(),
                    //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    //        filename);
                    //}
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ExportExcel)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Tải mẫu file Excel danh sách khách hàng
        /// </summary>
        /// <remarks>
        /// API này tạo và trả về file Excel mẫu để nhập thông tin khách hàng cho một gói thầu cụ thể.
        /// Tên file sẽ dựa trên ngày hiện tại.
        /// </remarks>
        /// <param name="id">ID của gói thầu cần tải mẫu danh sách khách hàng</param>
        /// <returns>File Excel mẫu để nhập thông tin khách hàng</returns>
        /// <response code="200">Thành công, trả về file Excel mẫu</response>
        /// <response code="404">Không tìm thấy gói thầu với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("template-list-customers/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetTemplateListCustomers([FromRoute] string id)
        {
            try
            {
                var data = await _procurementRepository.GetTemplateListCustomers(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetTemplateListCustomers)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Nhập danh sách khách hàng từ file Excel
        /// </summary>
        /// <remarks>
        /// API này thực hiện nhập danh sách khách hàng cho một gói thầu từ file Excel.
        /// 
        /// Form Data:
        /// - procurementId: ID của gói thầu cần nhập danh sách khách hàng
        /// - requestedBy: ID của người yêu cầu nhập danh sách (tự động lấy từ user hiện tại)
        /// - file: File Excel chứa danh sách khách hàng
        /// </remarks>
        /// <param name="procurementId">ID của gói thầu</param>
        /// <param name="requestedBy">ID của người yêu cầu</param>
        /// <param name="file">File Excel chứa danh sách khách hàng</param>
        /// <returns>Kết quả nhập danh sách khách hàng</returns>
        /// <response code="200">Thành công, đã nhập danh sách khách hàng</response>
        /// <response code="400">File không hợp lệ hoặc dữ liệu không đúng định dạng</response>
        /// <response code="404">Không tìm thấy gói thầu với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("import-list-customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ImportListCustomers([FromForm] string procurementId,
            [FromForm] string requestedBy,
            IFormFile file)
        {
            try
            {
                requestedBy = UserId;
                await _procurementRepository.ImportListCustomers(procurementId, requestedBy, file);
                return SaveSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ImportListCustomers)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Tạo chi tiết xuất cho gói thầu
        /// </summary>
        /// <remarks>
        /// API này thực hiện tạo mới một chi tiết xuất gia súc cho lô xuất thuộc gói thầu.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "batchExportId": "64f98c2d7b87af0c1839b7c1", // |ID của lô xuất|
        ///   "livestockId": "64f98c2d7b87af0c1839b7d1" // |ID của gia súc|
        /// }
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Tạo chi tiết xuất thành công",
        ///   "data": {
        ///     "batchExportId": "64f98c2d7b87af0c1839b7c1", // |ID của lô xuất|
        ///     "livestockId": "64f98c2d7b87af0c1839b7d1", // |ID của gia súc|
        ///     "requestedBy": "admin" // |ID của người tạo|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Thông tin chi tiết xuất cần tạo</param>
        /// <returns>Thông tin chi tiết xuất sau khi tạo</returns>
        /// <response code="200">Thành công, đã tạo chi tiết xuất</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy lô xuất hoặc gia súc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("create-export-detail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateExportDetail>> CreateExportDetail([FromBody] CreateExportDetail request)
        {
            try
            {
                request.RequestedBy = UserId;
                var data = await _procurementRepository.CreateExportDetail(request);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CreateExportDetail)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật con vật bàn giao của 1 user
        /// </summary>
        /// <remarks>
        /// API này thực hiện cập nhật thông tin của một chi tiết xuất gia súc đã tồn tại.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "batchExportDetailId": "64f98c2d7b87af0c1839b7e1", // |ID của chi tiết lô xuất|
        ///   "batchExportId": "64f98c2d7b87af0c1839b7c1", // |ID của lô xuất|
        ///   "livestockId": "64f98c2d7b87af0c1839b7d2" // |ID của gia súc mới|
        /// }
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Cập nhật chi tiết xuất thành công",
        ///   "data": {
        ///     "batchExportDetailId": "64f98c2d7b87af0c1839b7e1", // |ID của chi tiết lô xuất|
        ///     "batchExportId": "64f98c2d7b87af0c1839b7c1", // |ID của lô xuất|
        ///     "livestockId": "64f98c2d7b87af0c1839b7d2", // |ID của gia súc mới|
        ///     "requestedBy": "admin" // |ID của người cập nhật|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Thông tin cập nhật chi tiết xuất</param>
        /// <returns>Thông tin chi tiết xuất sau khi cập nhật</returns>
        /// <response code="200">Thành công, đã cập nhật chi tiết xuất</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy chi tiết xuất, lô xuất hoặc gia súc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("update-export-detail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UpdateExportDetail>> UpdateExportDetail([FromBody] UpdateExportDetail request)
        {
            try
            {
                request.RequestedBy = UserId;
                var data = await _procurementRepository.UpdateExportDetail(request);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateExportDetail)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách chi tiết xuất của lô xuất
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách chi tiết xuất của một lô xuất cụ thể, bao gồm thông tin về lô xuất và danh sách gia súc đã xuất.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Lấy danh sách chi tiết xuất thành công",
        ///   "data": {
        ///     "batchExportId": "64f98c2d7b87af0c1839b7c1", // |ID của lô xuất|
        ///     "customerName": "Công ty XYZ", // |Tên khách hàng|
        ///     "customerPhone": "0981234567", // |Số điện thoại khách hàng|
        ///     "customerAddress": "123 Đường ABC, TP.HCM", // |Địa chỉ khách hàng|
        ///     "customerNote": "Yêu cầu giao trong giờ hành chính", // |Ghi chú khách hàng|
        ///     "totalLivestocks": 5, // |Tổng số gia súc|
        ///     "received": 3, // |Số gia súc đã nhận|
        ///     "remaining": 2, // |Số gia súc còn lại|
        ///     "status": "ĐANG_BÀN_GIAO", // |Trạng thái lô xuất|
        ///     "total": 5, // |Tổng số chi tiết xuất|
        ///     "items": [ // |Danh sách chi tiết xuất|
        ///       {
        ///         "batchExportDetailId": "64f98c2d7b87af0c1839b7e1", // |ID chi tiết xuất|
        ///         "livestockId": "64f98c2d7b87af0c1839b7d1", // |ID gia súc|
        ///         "inspectionCode": "B01-001", // |Mã kiểm dịch gia súc|
        ///         "weightExport": 530.5, // |Cân nặng khi xuất (kg)|
        ///         "handoverDate": "2023-02-01T10:30:00", // |Ngày bàn giao|
        ///         "exportDate": "2023-02-01T09:15:00", // |Ngày xuất|
        ///         "expiredInsuranceDate": "2023-08-01T00:00:00", // |Ngày hết hạn bảo hành|
        ///         "status": "ĐÃ_BÀN_GIAO" // |Trạng thái chi tiết xuất|
        ///       },
        ///       {
        ///         "batchExportDetailId": "64f98c2d7b87af0c1839b7e2", // |ID chi tiết xuất|
        ///         "livestockId": "64f98c2d7b87af0c1839b7d2", // |ID gia súc|
        ///         "inspectionCode": "B01-002", // |Mã kiểm dịch gia súc|
        ///         "weightExport": 490.2, // |Cân nặng khi xuất (kg)|
        ///         "handoverDate": null, // |Ngày bàn giao|
        ///         "exportDate": "2023-02-02T11:20:00", // |Ngày xuất|
        ///         "expiredInsuranceDate": "2023-08-02T00:00:00", // |Ngày hết hạn bảo hành|
        ///         "status": "CHỜ_BÀN_GIAO" // |Trạng thái chi tiết xuất|
        ///       }
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của lô xuất cần xem chi tiết</param>
        /// <returns>Danh sách chi tiết xuất của lô xuất</returns>
        /// <response code="200">Thành công, trả về danh sách chi tiết xuất</response>
        /// <response code="404">Không tìm thấy lô xuất với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("list-export-details/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListExportDetails>> GetListExportDetails([FromRoute] string id)
        {
            try
            {
                var data = await _procurementRepository.GetListExportDetails(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListExportDetails)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách trạng thái gói thầu
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả các trạng thái gói thầu có trong hệ thống, bao gồm:
        /// - ĐANG_ĐẤU_THẦU
        /// - CHỜ_BÀN_GIAO
        /// - ĐANG_BÀN_GIAO
        /// - HOÀN_THÀNH
        /// - ĐÃ_HỦY
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     "ĐANG_ĐẤU_THẦU",
        ///     "CHỜ_BÀN_GIAO",
        ///     "ĐANG_BÀN_GIAO",
        ///     "HOÀN_THÀNH",
        ///     "ĐÃ_HỦY"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách các trạng thái gói thầu</returns>
        /// <response code="200">Thành công, trả về danh sách trạng thái</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-procurement-statuses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetListProcurementStatuses()
        {
            try
            {
                var data = Enum.GetNames(typeof(procurement_status)).ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListProcurementStatuses)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
