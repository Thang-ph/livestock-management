using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.AutoMapperConfig;
using DataAccess.Repository.Interfaces;
using DataAccess.Repository.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    /// <summary>
    /// API quản lý tiêm phòng và tiêm vacxin trong hệ thống
    /// </summary>
    [Route("api/vaccination-management")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý tiêm phòng: lập kế hoạch, thực hiện, và theo dõi các đợt tiêm phòng")]
    public class BatchVacinationController : BaseAPIController
    {
        private readonly IBatchVacinationRepository _batchVacinationRepository;
        private readonly ILogger<BatchVacinationController> _logger;
        public BatchVacinationController(ILogger<BatchVacinationController> logger, IBatchVacinationRepository batchVacinationRepository)
        {
            _batchVacinationRepository = batchVacinationRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách trạng thái của đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các trạng thái của đợt tiêm phòng trong hệ thống, bao gồm:
        /// - CHỜ_THỰC_HIỆN: Đợt tiêm phòng mới được tạo, chưa bắt đầu
        /// - ĐANG_THỰC_HIỆN: Đợt tiêm phòng đang được tiến hành
        /// - HOÀN_THÀNH: Đợt tiêm phòng đã được thực hiện xong
        /// - ĐÃ_HỦY: Đợt tiêm phòng đã bị hủy bỏ
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": [
        ///     "CHỜ_THỰC_HIỆN", // |Trạng thái chờ thực hiện|
        ///     "ĐANG_THỰC_HIỆN", // |Trạng thái đang tiến hành|
        ///     "HOÀN_THÀNH", // |Trạng thái đã hoàn thành|
        ///     "ĐÃ_HỦY" // |Trạng thái đã hủy|
        ///   ],
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Get Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách các trạng thái tiêm phòng</returns>
        /// <response code="200">Thành công, trả về danh sách trạng thái</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-vaccination-statuses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetListVaccinationStatuses()
        {
            try
            {
                var data = Enum.GetNames(typeof(batch_vaccination_status)).ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListVaccinationStatuses)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách loại tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các loại tiêm phòng trong hệ thống, bao gồm:
        /// - TIÊM_VACCINE: Tiêm phòng để ngăn ngừa bệnh
        /// - TIÊM_CHỮA_BỆNH: Tiêm thuốc để điều trị bệnh
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": [
        ///     "TIÊM_VACCINE", // |Loại tiêm phòng ngừa|
        ///     "TIÊM_CHỮA_BỆNH" // |Loại tiêm điều trị|
        ///   ],
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Get Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách các loại tiêm phòng</returns>
        /// <response code="200">Thành công, trả về danh sách loại tiêm phòng</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-vaccination-types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetListVaccinationTypes()
        {
            try
            {
                var data = Enum.GetNames(typeof(batch_vaccination_type)).ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListVaccinationTypes)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Tạo đợt tiêm phòng mới
        /// </summary>
        /// <remarks>
        /// API này tạo một đợt tiêm phòng mới trong hệ thống với thông tin chi tiết về lịch trình và vaccine sử dụng.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "dateSchedule": "2025-04-15T08:00:00", // |Ngày dự kiến tiêm phòng|
        ///   "name": "Tiêm phòng Lở mồm long móng đợt 1 năm 2025", // |Tên đợt tiêm phòng|
        ///   "conductedBy": "Nguyễn Văn A", // |Người thực hiện tiêm phòng|
        ///   "vaccineId": "vac-001", // |ID của vaccine sử dụng|
        ///   "description": "Tiêm phòng định kỳ cho đàn bò", // |Mô tả chi tiết|
        ///   "type": "TIÊM_VACCINE", // |Loại tiêm phòng (TIÊM_VACCINE hoặc TIÊM_CHỮA_BỆNH)|
        ///   "status": "CHỜ_THỰC_HIỆN", // |Trạng thái ban đầu của đợt tiêm phòng|
        ///   "createdBy": "user-001" // |ID người tạo đợt tiêm phòng|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": {
        ///     "id": "BV-2025-001", // |ID của đợt tiêm phòng|
        ///     "name": "Tiêm phòng Lở mồm long móng đợt 1 năm 2025", // |Tên đợt tiêm phòng|
        ///     "dateSchedule": "2025-04-15T08:00:00", // |Ngày dự kiến tiêm phòng|
        ///     "conductedBy": "Nguyễn Văn A", // |Người thực hiện tiêm phòng|
        ///     "vaccineId": "vac-001", // |ID của vaccine sử dụng|
        ///     "description": "Tiêm phòng định kỳ cho đàn bò", // |Mô tả chi tiết|
        ///     "type": "TIÊM_VACCINE", // |Loại tiêm phòng|
        ///     "status": "CHỜ_THỰC_HIỆN", // |Trạng thái đợt tiêm phòng|
        ///     "createdAt": "2025-03-01T09:30:00", // |Thời gian tạo|
        ///     "createdBy": "user-001" // |ID người tạo đợt tiêm phòng|
        ///   },
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Save Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="batchVacinationCreate">Thông tin đợt tiêm phòng cần tạo, bao gồm ngày tiêm, người thực hiện, vaccine sử dụng, loại và trạng thái</param>
        /// <returns>Thông tin đợt tiêm phòng sau khi tạo</returns>
        /// <response code="200">Thành công, trả về thông tin đợt tiêm phòng đã tạo</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("create-vacination-batch-details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateVacinationBatch([FromBody] BatchVacinationCreate batchVacinationCreate)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(CreateVacinationBatch)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var batchVacination = await _batchVacinationRepository.CreateBatchVacinationDetail(batchVacinationCreate);
                return SaveSuccess(batchVacination);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CreateVacinationBatch)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này cập nhật thông tin của một đợt tiêm phòng đã tồn tại trong hệ thống.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Tiêm phòng Lở mồm long móng đợt 1 năm 2025 - Đàn A", // |Tên mới của đợt tiêm phòng|
        ///   "dateSchedule": "2025-04-17T08:00:00", // |Ngày dự kiến mới|
        ///   "conductedBy": "Nguyễn Văn A", // |Người thực hiện tiêm phòng|
        ///   "vaccineId": "vac-001", // |ID vaccine sử dụng|
        ///   "description": "Tiêm phòng định kỳ cho đàn bò khu vực A", // |Mô tả chi tiết cập nhật|
        ///   "type": "TIÊM_VACCINE", // |Loại tiêm phòng (TIÊM_VACCINE hoặc TIÊM_CHỮA_BỆNH)|
        ///   "updatedBy": "user-001" // |ID người cập nhật|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": {
        ///     "id": "BV-2025-001", // |ID của đợt tiêm phòng|
        ///     "name": "Tiêm phòng Lở mồm long móng đợt 1 năm 2025 - Đàn A", // |Tên đợt tiêm phòng|
        ///     "dateSchedule": "2025-04-17T08:00:00", // |Ngày dự kiến tiêm phòng|
        ///     "conductedBy": "Nguyễn Văn A", // |Người thực hiện tiêm phòng|
        ///     "vaccineId": "vac-001", // |ID của vaccine sử dụng|
        ///     "description": "Tiêm phòng định kỳ cho đàn bò khu vực A", // |Mô tả chi tiết|
        ///     "type": "TIÊM_VACCINE", // |Loại tiêm phòng|
        ///     "status": "CHỜ_THỰC_HIỆN", // |Trạng thái đợt tiêm phòng|
        ///     "updatedAt": "2025-03-10T11:15:00", // |Thời gian cập nhật|
        ///     "updatedBy": "user-001" // |ID người cập nhật|
        ///   },
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Save Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="batchVaccinationId">ID của đợt tiêm phòng cần cập nhật</param>
        /// <param name="batchVacinationUpdate">Thông tin cập nhật bao gồm tên, ngày dự kiến, người thực hiện, loại tiêm phòng và vaccine sử dụng</param>
        /// <returns>Thông tin đợt tiêm phòng sau khi cập nhật</returns>
        /// <response code="200">Thành công, trả về thông tin đợt tiêm phòng đã cập nhật</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy đợt tiêm phòng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("update-vacination-batch-details/{batchVaccinationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateVacinationBatch([FromBody] BatchVacinationUpdate batchVacinationUpdate, [FromRoute] string batchVaccinationId)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(UpdateVacinationBatch)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var batchVacination = await _batchVacinationRepository.UpdateBatchVacinationAsync(batchVacinationUpdate, batchVaccinationId);
                return SaveSuccess(batchVacination);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateVacinationBatch)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các đợt tiêm phòng theo các tiêu chí lọc.
        /// 
        /// Các tham số lọc:
        /// - FromDate: Ngày bắt đầu khoảng thời gian tìm kiếm (định dạng yyyy-MM-dd)
        /// - ToDate: Ngày kết thúc khoảng thời gian tìm kiếm (định dạng yyyy-MM-dd)
        /// - Status: Trạng thái đợt tiêm phòng (Các giá trị có thể: CHỜ_THỰC_HIỆN, ĐANG_THỰC_HIỆN, HOÀN_THÀNH, ĐÃ_HỦY)
        /// - PageSize: Số lượng bản ghi trên mỗi trang
        /// - PageNumber: Số trang
        /// - Keyword: Từ khóa tìm kiếm theo tên
        /// 
        /// Ví dụ Query Parameters: (từ và sẽ thay thế cho kí hiệu "và" vì không thể dùng đươc)
        /// ```
        /// ?FromDate=2025-04-01 và ToDate=2025-05-31 và Status=CHỜ_THỰC_HIỆN và PageSize=10 và PageNumber=1 và Keyword=Lở%20mồm
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "data": {
        ///     "total": 2,
        ///     "items": [
        ///       {
        ///         "id": "BV-2025-001", // |ID của đợt tiêm phòng|
        ///         "name": "Tiêm phòng Lở mồm long móng đợt 1 năm 2025 - Đàn A", // |Tên đợt tiêm phòng|
        ///         "medcicalType": "VACCINE", // |Loại thuốc/vaccine sử dụng|
        ///         "symptom": "Lở loét miệng, chân", // |Triệu chứng bệnh cần điều trị/phòng ngừa|
        ///         "dateSchedule": "2025-04-17T08:00:00", // |Ngày dự kiến tiêm phòng|
        ///         "conductedBy": "Nguyễn Văn A", // |Người thực hiện tiêm phòng|
        ///         "status": "CHỜ_THỰC_HIỆN", // |Trạng thái đợt tiêm phòng|
        ///         "dateConduct": null, // |Ngày thực hiện tiêm phòng (nếu đã thực hiện)|
        ///         "createdAt": "2025-03-01T09:30:00" // |Thời gian tạo đợt tiêm phòng|
        ///       },
        ///       {
        ///         "id": "BV-2025-002", // |ID của đợt tiêm phòng|
        ///         "name": "Tiêm phòng Lở mồm long móng đợt 1 năm 2025 - Đàn B", // |Tên đợt tiêm phòng|
        ///         "medcicalType": "VACCINE", // |Loại thuốc/vaccine sử dụng|
        ///         "symptom": "Lở loét miệng, chân", // |Triệu chứng bệnh cần điều trị/phòng ngừa|
        ///         "dateSchedule": "2025-05-10T08:00:00", // |Ngày dự kiến tiêm phòng|
        ///         "conductedBy": "Trần Thị B", // |Người thực hiện tiêm phòng|
        ///         "status": "CHỜ_THỰC_HIỆN", // |Trạng thái đợt tiêm phòng|
        ///         "dateConduct": null, // |Ngày thực hiện tiêm phòng (nếu đã thực hiện)|
        ///         "createdAt": "2025-03-05T14:20:00" // |Thời gian tạo đợt tiêm phòng|
        ///       }
        ///     ]
        ///   },
        ///   "errors": null,
        ///   "message": "Get Success"
        /// }
        /// ```
        /// </remarks>
        /// <param name="filter">Tiêu chí lọc (FromDate, ToDate, Status, PageSize, PageNumber, Keyword)</param>
        /// <returns>Danh sách đợt tiêm phòng phù hợp với tiêu chí lọc</returns>
        /// <response code="200">Thành công, trả về danh sách đợt tiêm phòng</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-batch-vaccinations-list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListVaccination>> GetBatchVaccinationsList([FromQuery] ListVaccinationsFliter filter)
        {
            try
            {
                var data = await _batchVacinationRepository.GetListVaccinations(filter);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetBatchVaccinationsList)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin chi tiết của một đợt tiêm phòng.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///  "statusCode": 200, // |Mã trạng thái HTTP|
        ///  "success": true, // |Trạng thái thành công của request|
        ///  "data": {
        ///   "id": "BV-2025-001", // |ID của đợt tiêm phòng|
        ///   "name": "Tiêm Phòng Lở Mồm Long Móng", // |Tên đợt tiêm phòng|
        ///   "vaccinationType": "TIÊM_VACCINE", // |Loại tiêm phòng (TIÊM_VACCINE hoặc TIÊM_CHỮA_BỆNH)|
        ///   "medcicalType": "VACCINE", // |Loại thuốc/vaccine sử dụng|
        ///   "symptom": "Lở loét miệng, chân", // |Triệu chứng bệnh cần điều trị/phòng ngừa|
        ///   "dateSchedule": "2025-04-15T08:00:00", // |Ngày dự kiến tiêm phòng|
        ///   "status": "CHỜ_THỰC_HIỆN", // |Trạng thái đợt tiêm phòng|
        ///   "conductedBy": "Nguyễn Văn A", // |Người thực hiện tiêm phòng|
        ///   "note": "Tiêm phòng định kỳ cho đàn bò khu vực A" // |Ghi chú bổ sung|
        ///  },
        ///  "errors": null, // |Thông tin lỗi (nếu có)|
        ///  "message": "Get Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của đợt tiêm phòng cần xem chi tiết</param>
        /// <returns>Thông tin chi tiết đợt tiêm phòng</returns>
        /// <response code="200">Thành công, trả về thông tin chi tiết đợt tiêm phòng</response>
        /// <response code="404">Không tìm thấy đợt tiêm phòng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-batch-vaccinations-general-info/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VaccinationGeneral>> GetVaccinationGeneralInfo([FromRoute] string id)
        {
            try
            {
                var data = await _batchVacinationRepository.GetVaccinationGeneralInfo(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetVaccinationGeneralInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách gia súc được tiêm phòng trong đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các gia súc được tiêm phòng trong một đợt tiêm phòng.
        /// 
        /// Các tham số lọc:
        /// - FromDate: Ngày bắt đầu khoảng thời gian tìm kiếm (định dạng yyyy-MM-dd)
        /// - ToDate: Ngày kết thúc khoảng thời gian tìm kiếm (định dạng yyyy-MM-dd)
        /// - Status: Trạng thái gia súc (Các giá trị có thể: KHỎE_MẠNH, ỐM, CHỜ_XUẤT, ĐÃ_XUẤT, CHẾT)
        /// - PageSize: Số lượng bản ghi trên mỗi trang
        /// - PageNumber: Số trang
        /// - Keyword: Từ khóa tìm kiếm theo mã, tên gia súc
        /// 
        /// Ví dụ Query Parameters:
        /// ```
        /// ?FromDate=2025-04-01 và ToDate=2025-05-31 và Status=KHỎE_MẠNH và PageSize=10 và PageNumber=1 và Keyword=Bò%20sữa
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///  "statusCode": 200, 
        ///  "success": true,
        ///  "data": {
        ///    "total": 2,
        ///    "items": [
        ///      {
        ///        "id": "LS001", // |ID của gia súc|
        ///        "inspectionCode": "000003", // |Mã kiểm định của gia súc|
        ///        "species": "Bò Sahiwal", // |Loài/giống gia súc|
        ///        "color": "Trắng", // |Màu sắc gia súc|
        ///        "status": "KHỎE_MẠNH", // |Trạng thái gia súc|
        ///        "injections_count": 1, // |Số lần tiêm phòng đã thực hiện|
        ///        "dateConduct": "2025-04-17T10:30:00", // |Ngày thực hiện tiêm phòng|
        ///        "createdAt": "2025-04-17T10:30:00" // |Thời gian tạo bản ghi|
        ///      },
        ///      {
        ///        "id": "LS002", // |ID của gia súc|
        ///        "inspectionCode": "000028", // |Mã kiểm định của gia súc|
        ///        "species": "Bò lai Sind", // |Loài/giống gia súc|
        ///        "color": "Trắng", // |Màu sắc gia súc|
        ///        "status": "KHỎE_MẠNH", // |Trạng thái gia súc|
        ///        "injections_count": 2, // |Số lần tiêm phòng đã thực hiện|
        ///        "dateConduct": "2025-04-17T11:15:00", // |Ngày thực hiện tiêm phòng|
        ///        "createdAt": "2025-04-17T11:15:00" // |Thời gian tạo bản ghi|
        ///      }
        ///    ]
        ///  },
        ///  "errors": null,
        ///  "message": "Get Success"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của đợt tiêm phòng</param>
        /// <param name="filter">Tiêu chí lọc (FromDate, ToDate, Status, PageSize, PageNumber, Keyword)</param>
        /// <returns>Danh sách gia súc được tiêm phòng trong đợt tiêm phòng</returns>
        /// <response code="200">Thành công, trả về danh sách gia súc được tiêm phòng</response>
        /// <response code="404">Không tìm thấy đợt tiêm phòng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-batch-livestock-vaccinations-list/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListLivestocksVaccination>> GetVaccinationLivestocksGeneralInfo([FromRoute] string id, [FromQuery] ListLivestockVaccination filter)
        {
            try
            {
                var data = await _batchVacinationRepository.GetListLivestockVaccination(id, filter);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetVaccinationLivestocksGeneralInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Hủy đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này thực hiện hủy một đợt tiêm phòng đã tạo, chuyển trạng thái từ CHỜ_THỰC_HIỆN hoặc ĐANG_THỰC_HIỆN sang ĐÃ_HỦY.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "vaccinationBatchId": "BV-2025-001", // |ID của đợt tiêm phòng cần hủy|
        ///   "requestedBy": "user-001" // |ID của người yêu cầu hủy|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": true, // |Kết quả thực hiện (true: thành công)|
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Save Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "statusCode": 400, // |Mã trạng thái HTTP|
        ///   "success": false, // |Trạng thái thành công của request|
        ///   "data": "Không thể hủy đợt tiêm phòng đã hoàn thành", // |Thông báo lỗi|
        ///   "errors": null, // |Thông tin lỗi bổ sung (nếu có)|
        ///   "message": "Save Data Failed" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Thông tin hủy đợt tiêm phòng (vaccinationBatchId: ID đợt tiêm phòng, requestedBy: người yêu cầu hủy)</param>
        /// <returns>Kết quả hủy đợt tiêm phòng</returns>
        /// <response code="200">Thành công, đã hủy đợt tiêm phòng</response>
        /// <response code="400">Không thể hủy do trạng thái không phù hợp</response>
        /// <response code="404">Không tìm thấy đợt tiêm phòng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CancelVaccinationBatch([FromBody] ChangeVaccinationBatchStatus request)
        {
            try
            {
                var data = await _batchVacinationRepository.CancelVaccinationBatch(request);
                if (data)
                    return SaveSuccess(data);
                return SaveError("Cancel vaccination batch failed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CancelVaccinationBatch)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Hoàn thành đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này thực hiện đánh dấu một đợt tiêm phòng là đã hoàn thành, chuyển trạng thái từ ĐANG_THỰC_HIỆN sang HOÀN_THÀNH.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "vaccinationBatchId": "BV-2025-001", // |ID của đợt tiêm phòng cần hoàn thành|
        ///   "requestedBy": "user-001" // |ID của người yêu cầu hoàn thành|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": true, // |Kết quả thực hiện (true: thành công)|
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Save Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "statusCode": 400, // |Mã trạng thái HTTP|
        ///   "success": false, // |Trạng thái thành công của request|
        ///   "data": "Không thể hoàn thành đợt tiêm phòng có trạng thái CHỜ_THỰC_HIỆN", // |Thông báo lỗi|
        ///   "errors": null, // |Thông tin lỗi bổ sung (nếu có)|
        ///   "message": "Save Data Failed" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Thông tin hoàn thành đợt tiêm phòng (vaccinationBatchId: ID đợt tiêm phòng, requestedBy: người yêu cầu hoàn thành)</param>
        /// <returns>Kết quả hoàn thành đợt tiêm phòng</returns>
        /// <response code="200">Thành công, đã hoàn thành đợt tiêm phòng</response>
        /// <response code="400">Không thể hoàn thành do trạng thái không phù hợp</response>
        /// <response code="404">Không tìm thấy đợt tiêm phòng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CompleteVaccinationBatch([FromBody] ChangeVaccinationBatchStatus request)
        {
            try
            {
                var data = await _batchVacinationRepository.CompleteVaccinationBatch(request);
                if (data)
                    return SaveSuccess(data);
                return SaveError("Complete vaccination batch failed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CompleteVaccinationBatch)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin gia súc theo mã QR
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin của một gia súc dựa trên mã QR được quét hoặc ID/mã định danh.
        /// 
        /// Các tham số truy vấn:
        /// - LivestockId: ID của gia súc (nếu có)
        /// - InspectionCode: Mã kiểm định của gia súc (nếu có)
        /// - SpecieType: Loại gia súc (TRÂU, BÒ, LỢN, GÀ, DÊ, CỪU, NGỰA, LA, LỪA)
        /// 
        /// Ví dụ Query Parameters:
        /// ```
        /// ?InspectionCode=000028 và SpecieType=BÒ
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///  "statusCode": 200,
        ///  "success": true,
        ///  "data": {
        ///    "livestockId": "LS001", // |ID của gia súc|
        ///    "inspectionCode": "000028", // |Mã định danh của gia súc|
        ///    "specieName": "Bò lai Sind", // |Tên loài/giống gia súc|
        ///    "color": "Trắng", // |Màu sắc gia súc|
        ///    "vaccinationInfos": [
        ///      {
        ///        "diseaseName": "Lở mồm long móng", // |Tên bệnh đã tiêm phòng|
        ///        "numberOfVaccination": 2 // |Số lần tiêm phòng cho bệnh này|
        ///      },
        ///      {
        ///        "diseaseName": "Viêm da nổi cục", // |Tên bệnh đã tiêm phòng|
        ///        "numberOfVaccination": 1 // |Số lần tiêm phòng cho bệnh này|
        ///      }
        ///    ]
        ///  },
        ///  "errors": null,
        ///  "message": "Get Success"
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Thông tin quét mã QR (LivestockId, InspectionCode, SpecieType)</param>
        /// <returns>Thông tin gia súc theo mã QR</returns>
        /// <response code="200">Thành công, trả về thông tin gia súc</response>
        /// <response code="404">Không tìm thấy gia súc với mã QR cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-livestock-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockVaccinationInfo>> GetLivestockInfo([FromQuery] ScanLivestockQrCode request)
        {
            try
            {
                var data = await _batchVacinationRepository.GetLivestockInfo(request);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLivestockInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Thêm gia súc vào đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này thêm một gia súc vào đợt tiêm phòng, chuyển trạng thái từ CHỜ_THỰC_HIỆN sang ĐANG_THỰC_HIỆN.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "vaccinationBatchId": "BV-2025-001", // |ID của đợt tiêm phòng|
        ///   "livestockId": "LS001" // |ID của gia súc cần thêm|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": true, // |Kết quả thực hiện (true: thành công)|
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Save Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "statusCode": 400, // |Mã trạng thái HTTP|
        ///   "success": false, // |Trạng thái thành công của request|
        ///   "data": "Không thể thêm gia súc vào đợt tiêm phòng", // |Thông báo lỗi|
        ///   "errors": null, // |Thông tin lỗi bổ sung (nếu có)|
        ///   "message": "Save Data Failed" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Thông tin thêm gia súc vào đợt tiêm phòng</param>
        /// <returns>Kết quả thêm gia súc vào đợt tiêm phòng</returns>
        /// <response code="200">Thành công, đã thêm gia súc vào đợt tiêm phòng</response>
        /// <response code="400">Không thể thêm gia súc vào đợt tiêm phòng</response>
        /// <response code="404">Không tìm thấy đợt tiêm phòng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("add-to-vaccination-batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Obsolete("This API is old version")]
        public async Task<ActionResult<bool>> AddToVacciantionBatch([FromBody] AddLivestockToVaccinationBatch request)
        {
            try
            {
                var data = await _batchVacinationRepository.AddToVaccinationBatch(request);
                if (data)
                    return SaveSuccess(data);
                return SaveError("Add livestock to vaccination batch failed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddToVacciantionBatch)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Thêm thông tin tiêm chủng cho gia súc vào đợt tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này thêm thông tin chi tiết về việc tiêm chủng cho một gia súc cụ thể vào đợt tiêm phòng.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "livestockId": "LS001", // |ID của gia súc cần tiêm chủng|
        ///   "batchVaccinationId": "BV-2025-001", // |ID của đợt tiêm phòng|
        ///   "createdBy": "user-001" // |ID người thực hiện tiêm chủng|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": {
        ///     "id": "LV-2025-001", // |ID của bản ghi tiêm chủng|
        ///     "livestockId": "LS001", // |ID của gia súc|
        ///     "batchVaccinationId": "BV-2025-001", // |ID của đợt tiêm phòng|
        ///     "createdAt": "2025-04-17T10:30:00", // |Thời gian tạo|
        ///     "createdBy": "user-001" // |ID người thực hiện tiêm chủng|
        ///   },
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Save Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="livestockVaccinationAdd">Thông tin tiêm chủng cho gia súc</param>
        /// <returns>Thông tin chi tiết về việc tiêm chủng đã thêm</returns>
        /// <response code="200">Thành công, trả về thông tin chi tiết về việc tiêm chủng</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy gia súc hoặc đợt tiêm phòng</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("add-livestock-vacination-to-vacination-batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockVaccinationAdd>> Create([FromBody] LivestockVaccinationAdd livestockVaccinationAdd)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Create)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                LivestockVaccination livestockVaccination = await _batchVacinationRepository.AddLivestockVacinationToVacinationBatch(livestockVaccinationAdd);
                return SaveSuccess(livestockVaccination);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
        [HttpPost("add-livestock-vacination-to-vacination-batch-by-inspection-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockVaccinationAdd>> AddLivestockVaccinationToVaccinationBatch([FromBody] LivestockVaccinationAddByInspectionCode livestockVaccinationAddByInspectionCode)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Create)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                LivestockVaccination livestockVaccination = await _batchVacinationRepository.AddLivestockVaccinationToVaccinationBatch(livestockVaccinationAddByInspectionCode);
                return SaveSuccess(livestockVaccination);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddLivestockVaccinationToVaccinationBatch)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
        /// <summary>
        /// Lấy thông tin tiêm phòng của gia súc theo ID
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin tiêm phòng của gia súc dựa theo ID gia súc.
        /// API này đã lỗi thời và sẽ bị loại bỏ trong phiên bản tới.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": {
        ///     "id": "LS001", // |ID của gia súc|
        ///     "inspectionCode": "000028", // |Mã kiểm định của gia súc|
        ///     "species": "Bò lai Sind", // |Loài/giống gia súc|
        ///     "color": "Trắng", // |Màu sắc gia súc|
        ///     "status": "KHỎE_MẠNH", // |Trạng thái gia súc|
        ///     "injections_count": [ // |Danh sách các lần tiêm phòng|
        ///       {
        ///         "injections_count": 1, // |Số lần tiêm phòng cho bệnh này|
        ///         "diseaseName": "Lở mồm long móng", // |Tên bệnh|
        ///         "name": "Tiêm phòng Lở mồm long móng đợt 1 năm 2025", // |Tên đợt tiêm phòng|
        ///         "conductedBy": "Nguyễn Văn A", // |Người thực hiện tiêm phòng|
        ///         "description": "Tiêm phòng định kỳ" // |Mô tả chi tiết|
        ///       },
        ///       {
        ///         "injections_count": 1, // |Số lần tiêm phòng cho bệnh này|
        ///         "diseaseName": "Viêm da nổi cục", // |Tên bệnh|
        ///         "name": "Tiêm phòng Viêm da nổi cục đợt 2 năm 2024", // |Tên đợt tiêm phòng|
        ///         "conductedBy": "Trần Thị B", // |Người thực hiện tiêm phòng|
        ///         "description": "Tiêm phòng theo đợt" // |Mô tả chi tiết|
        ///       }
        ///     ],
        ///     "dateConduct": "2025-04-17T10:30:00", // |Ngày thực hiện tiêm phòng gần nhất|
        ///     "createdAt": "2025-03-01T09:30:00" // |Thời gian tạo bản ghi|
        ///   },
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Get Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="livestockId">ID của gia súc</param>
        /// <returns>Lịch sử tiêm phòng của gia súc</returns>
        /// <response code="200">Thành công, trả về lịch sử tiêm phòng của gia súc</response>
        /// <response code="404">Không tìm thấy gia súc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [Obsolete("This API is old version")]
        [HttpGet("get-livestock-vacination-info-by-id/{livestockId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VaccinationGeneral>> GetLivestockVaccinationByID([FromRoute] string livestockId)
        {
            try
            {
                var data = await _batchVacinationRepository.GetLivestockVaccinationByID(livestockId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLivestockVaccinationByID)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên có thể thực hiện tiêm phòng
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các nhân viên và quản lý có thể thực hiện tiêm phòng vào ngày được chỉ định.
        /// 
        /// Tham số truy vấn:
        /// - DateSchedule: Ngày dự kiến tiêm phòng (định dạng yyyy-MM-dd)
        /// 
        /// Ví dụ Query Parameters:
        /// ```
        /// ?DateSchedule=2025-04-17
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "statusCode": 200, // |Mã trạng thái HTTP|
        ///   "success": true, // |Trạng thái thành công của request|
        ///   "data": [
        ///     {
        ///       "id": "user-001", // |ID của người dùng|
        ///       "fullName": "Nguyễn Văn A", // |Họ tên đầy đủ|
        ///       "role": "NHÂN_VIÊN", // |Vai trò trong hệ thống|
        ///       "phone": "0912345678", // |Số điện thoại|
        ///       "email": "nguyenvana@example.com" // |Email liên hệ|
        ///     },
        ///     {
        ///       "id": "user-002", // |ID của người dùng|
        ///       "fullName": "Trần Thị B", // |Họ tên đầy đủ|
        ///       "role": "QUẢN_LÝ", // |Vai trò trong hệ thống|
        ///       "phone": "0987654321", // |Số điện thoại|
        ///       "email": "tranthib@example.com" // |Email liên hệ|
        ///     }
        ///   ],
        ///   "errors": null, // |Thông tin lỗi (nếu có)|
        ///   "message": "Get Success" // |Thông báo kết quả|
        /// }
        /// ```
        /// </remarks>
        /// <param name="DateSchedule">Ngày dự kiến tiêm phòng</param>
        /// <returns>Danh sách nhân viên và quản lý có thể thực hiện tiêm phòng</returns>
        /// <response code="200">Thành công, trả về danh sách nhân viên và quản lý</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-conductor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserDTO>>> GetStaffAndManagerUser(DateTime DateSchedule)
        {
            try
            {
                List<UserDTO> result = await _batchVacinationRepository.GetStaffAndManagerUserAsync(DateSchedule);
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetStaffAndManagerUser)} " + ex.Message);
                return GetError("Đã xảy ra lỗi khi lấy danh sách nhân viên và quản lý: " + ex.Message);
            }
        }

        /// <summary>
        /// Xóa thông tin tiêm phòng của gia súc
        /// </summary>
        /// <remarks>
        /// API này xóa thông tin tiêm phòng của một gia súc khỏi đợt tiêm phòng.
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "data": true, // |Kết quả thực hiện (true: thành công)|
        ///   "errors": null,
        ///   "message": "Save Success"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "statusCode": 400,
        ///   "success": false,
        ///   "data": "Không thể xóa thông tin tiêm phòng của gia súc",
        ///   "errors": null,
        ///   "message": "Save Data Failed"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của bản ghi tiêm phòng gia súc cần xóa</param>
        /// <returns>Kết quả xóa thông tin tiêm phòng</returns>
        /// <response code="200">Thành công, đã xóa thông tin tiêm phòng</response>
        /// <response code="404">Không tìm thấy thông tin tiêm phòng với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpDelete("delete-livestock-batch-vaccination/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeleteLiveStockBatchvaccination([FromRoute] string id)
        {
            try
            {
                var result = await _batchVacinationRepository.DeleteLiveStockVaccination(id);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(DeleteLiveStockBatchvaccination)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
        [HttpGet("export-template-vaccination-batch")]
        public async Task<IActionResult> ExportTemplateVaccinationBatch()
        {
            try
            {
                Stream result = await _batchVacinationRepository.ExportTemplateVaccinationBatch();

                return File(result,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "Mẫu lô tiêm.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ExportTemplateVaccinationBatch)} " + ex.Message);
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        [HttpPost("import-list-livstock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ImportListLivestock([FromForm] string batchVaccinId, [FromForm] string requestedBy, IFormFile file)
        {
            try
            {
                var data = await _batchVacinationRepository.ImportListLivestock(batchVaccinId, requestedBy, file);
                return SaveSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ImportListLivestock)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
        [HttpGet("get-list-procurement-require-vaccination")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ListRequireVaccinationProcurement>>> GetListProcurementRequireVaccination(
           [FromQuery] string? procurementSearch,
           [FromQuery] OrderBy? orderBy,
           [FromQuery] DateTime? fromDate,
           [FromQuery] DateTime? toDate)
        {
            try
            {
                var data = await _batchVacinationRepository.GetListProcurementRequireVaccination(procurementSearch, orderBy, fromDate, toDate);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListProcurementRequireVaccination)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("get-list-suggest-re-vaccination")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ListSuggestReVaccination>>> GetListSuggestReVaccination(
          [FromQuery] string? search,
          [FromQuery] string? medicineId,
          [FromQuery] string? diseaseId,
          [FromQuery] DateTime? fromDate,
          [FromQuery] DateTime? toDate)
        {
            try
            {
                var data = await _batchVacinationRepository.GetListSuggestReVaccination(search, medicineId, diseaseId, fromDate, toDate);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListSuggestReVaccination)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("get-list-future-vaccination")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ListSuggestReVaccination>>> GetListFutureVaccination(
         [FromQuery] string? search,
          [FromQuery] string? diseaeId,
           [FromQuery] string? conductId,
         [FromQuery] DateTime? fromDate,
         [FromQuery] DateTime? toDate)
        {
            try
            {
                var data = await _batchVacinationRepository.GetListFutureVaccination(search,diseaeId,conductId, fromDate, toDate);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListFutureVaccination)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("get-procurement-require-vaccination/{procurementId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequireVaccinationProcurementDetail>> GetProcurementRequireVaccinationDetail( string procurementId )
        {
            try
            {
                var data = await _batchVacinationRepository.GetProcurementRequireVaccinationDetail(procurementId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetProcurementRequireVaccinationDetail)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("get-livestock-require-vaccination-for-procurement/{procurementId}/{livestockId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequireVaccinationProcurementDetail>> GetLivestockRequirementForProcurement(string livestockId,string procurementId)
        {
            try
            {
                var data = await _batchVacinationRepository.GetLivestockRequirementForProcurement(livestockId, procurementId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLivestockRequirementForProcurement)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("get-livestock-require-vaccination-for-procurement/{procurementId}/{specieType}/{inspectionCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequireVaccinationProcurementDetail>> GetLivestockRequirementForProcurement(string inspectionCode, specie_type specieType, string procurementId)
        {
            try
            {
                var data = await _batchVacinationRepository.GetLivestockRequirementForProcurement(inspectionCode, specieType, procurementId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLivestockRequirementForProcurement)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
