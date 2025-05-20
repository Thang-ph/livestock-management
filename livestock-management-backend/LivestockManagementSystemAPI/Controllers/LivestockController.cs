using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using BusinessObjects;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using DataAccess.Repository.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    /// <summary>
    /// API quản lý thông tin gia súc trong hệ thống
    /// </summary>
    [Route("api/livestock-management")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý gia súc: tìm kiếm, xem chi tiết và lịch sử gia súc trong hệ thống")]
    public class LivestockController : BaseAPIController
    {
        private readonly ILivestockRepository _livestockRepository;
        private readonly ILogger<LivestockController> _logger;


        public LivestockController(ILivestockRepository livestockRepository, ILogger<LivestockController> logger)
        {
            _livestockRepository = livestockRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách gia súc theo các tiêu chí lọc
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách gia súc theo các tiêu chí lọc như cân nặng, loài, trạng thái.
        /// 
        /// Ví dụ Query Parameters:
        /// ```
        /// /api/livestock-management/get-all-livestock?minWeight=200 và maxWeight=500 và speciesIds=sp-001,sp-002 và statuses=KHỎE_MẠNH,ỐM và pageSize=10 và pageNumber=1 và keyword=B01
        /// ```
        /// 
        /// Trong đó:
        /// - minWeight: 200                     -> |Cân nặng tối thiểu của vật nuôi|
        /// - maxWeight: 500                     -> |Cân nặng tối đa của vật nuôi|
        /// - speciesIds: sp-001,sp-002          -> |Danh sách ID loài vật nuôi cần lọc|
        /// - statuses: KHỎE_MẠNH,ỐM             -> |Danh sách trạng thái vật nuôi cần lọc|
        /// - pageSize: 10                       -> |Số lượng kết quả mỗi trang|
        /// - pageNumber: 1                      -> |Trang hiện tại|
        /// - keyword: B01                       -> |Từ khóa tìm kiếm theo mã kiểm định|
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "total": 2,                          -> |Tổng số vật nuôi thỏa mãn điều kiện lọc|
        ///   "items": [
        ///     {
        ///       "id": "LS001",                   -> |ID của vật nuôi|
        ///       "inspectionCode": "B01-001",     -> |Mã kiểm định của vật nuôi|
        ///       "species": "Bò sữa",             -> |Tên loài/giống vật nuôi|
        ///       "weight": 450.5,                 -> |Cân nặng của vật nuôi (kg)|
        ///       "gender": "ĐỰC",                 -> |Giới tính của vật nuôi|
        ///       "color": "Đen trắng",            -> |Màu sắc vật nuôi|
        ///       "origin": "Lâm Đồng",            -> |Nguồn gốc xuất xứ vật nuôi|
        ///       "status": "KHỎE_MẠNH"            -> |Trạng thái vật nuôi|
        ///     },
        ///     {
        ///       "id": "LS002",                   -> |ID của vật nuôi|
        ///       "inspectionCode": "B01-002",     -> |Mã kiểm định của vật nuôi|
        ///       "species": "Bò thịt",            -> |Tên loài/giống vật nuôi|
        ///       "weight": 380.2,                 -> |Cân nặng của vật nuôi (kg)|
        ///       "gender": "CÁI",                 -> |Giới tính của vật nuôi|
        ///       "color": "Nâu",                  -> |Màu sắc vật nuôi|
        ///       "origin": "Hà Nội",              -> |Nguồn gốc xuất xứ vật nuôi|
        ///       "status": "KHỎE_MẠNH"            -> |Trạng thái vật nuôi|
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="filter">Tiêu chí lọc danh sách gia súc</param>
        /// <returns>Danh sách gia súc phù hợp với tiêu chí lọc</returns>
        /// <response code="200">Thành công, trả về danh sách gia súc</response>
        /// <response code="400">Lỗi dữ liệu đầu vào</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-all-livestock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListLivestocks>> GetListLivestocks([FromQuery] ListLivestocksFilter filter)
        {
            try
            {
                var data = await _livestockRepository.GetListLivestocks(filter);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListLivestocks)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin tóm tắt của gia súc
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin tóm tắt của một gia súc theo ID.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "id": "LS001",                       -> |ID của vật nuôi|
        ///   "inspectionCode": "B01-001",         -> |Mã kiểm định của vật nuôi|
        ///   "species": "Bò sữa",                 -> |Tên loài/giống vật nuôi|
        ///   "weight": 450.5,                     -> |Cân nặng của vật nuôi (kg)|
        ///   "gender": "ĐỰC",                     -> |Giới tính của vật nuôi|
        ///   "color": "Đen trắng",                -> |Màu sắc vật nuôi|
        ///   "origin": "Lâm Đồng",                -> |Nguồn gốc xuất xứ vật nuôi|
        ///   "status": "KHỎE_MẠNH"                -> |Trạng thái vật nuôi|
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của gia súc cần xem thông tin</param>
        /// <returns>Thông tin tóm tắt của gia súc</returns>
        /// <response code="200">Thành công, trả về thông tin gia súc</response>
        /// <response code="404">Không tìm thấy gia súc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-summary-info/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockGeneralInfo>> GetLiverstockSummaryInfo([FromRoute] string id)
        {
            try
            {
                var data = await _livestockRepository.GetLivestockSummaryInfo(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLiverstockSummaryInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của gia súc
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin chi tiết của một gia súc theo ID, bao gồm thông tin chuồng trại, nguồn gốc, và các thông tin nhập/xuất.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "id": "LS001",                       -> |ID của vật nuôi|
        ///   "inspectionCode": "B01-001",         -> |Mã kiểm định của vật nuôi|
        ///   "species": "Bò sữa",                 -> |Tên loài/giống vật nuôi|
        ///   "weight": 450.5,                     -> |Cân nặng của vật nuôi (kg)|
        ///   "gender": "ĐỰC",                     -> |Giới tính của vật nuôi|
        ///   "color": "Đen trắng",                -> |Màu sắc vật nuôi|
        ///   "origin": "Lâm Đồng",                -> |Nguồn gốc xuất xứ vật nuôi|
        ///   "status": "KHỎE_MẠNH",               -> |Trạng thái vật nuôi|
        ///   "barn": "Chuồng A1",                 -> |Tên chuồng nuôi vật nuôi|
        ///   "weightOrigin": 430.0,               -> |Cân nặng ban đầu khi nhập vật nuôi (kg)|
        ///   "weightExport": 0,                   -> |Cân nặng khi xuất vật nuôi (kg, nếu đã xuất)|
        ///   "dateImport": "2025-01-15T08:30:00", -> |Ngày nhập vật nuôi|
        ///   "dateExport": null,                  -> |Ngày xuất vật nuôi (nếu đã xuất)|
        ///   "batchImportId": "BI-2025-001",      -> |ID lô nhập vật nuôi|
        ///   "batchExportId": null,               -> |ID lô xuất vật nuôi (nếu đã xuất)|
        ///   "importedBy": "Nguyễn Văn A"         -> |Người thực hiện nhập vật nuôi|
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của gia súc cần xem thông tin</param>
        /// <returns>Thông tin chi tiết của gia súc</returns>
        /// <response code="200">Thành công, trả về thông tin chi tiết gia súc</response>
        /// <response code="404">Không tìm thấy gia súc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-general-info-of-livestock/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockGeneralInfo>> GetLiverstockGeneralInfo([FromRoute] string id)
        {
            try
            {
                var data = await _livestockRepository.GetLivestockGeneralInfo(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLiverstockGeneralInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("get-general-info-of-livestock/{inspectionCode}/{specieType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockGeneralInfo>> GetLiverstockGeneralInfo([FromRoute] string inspectionCode, [FromRoute] specie_type specieType)
        {
            try
            {
                var data = await _livestockRepository.GetLivestockGeneralInfo(inspectionCode, specieType);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLiverstockGeneralInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy lịch sử bệnh của gia súc
        /// </summary>
        /// <remarks>
        /// API này trả về lịch sử bệnh và điều trị của một gia súc theo ID.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "livestockId": "LS001",              -> |ID của vật nuôi|
        ///   "inspectionCode": "B01-001",         -> |Mã kiểm định của vật nuôi|
        ///   "total": 2,                          -> |Tổng số lần mắc bệnh|
        ///   "items": [
        ///     {
        ///       "dateOfRecord": "2025-02-10T09:15:00", -> |Thời gian phát hiện bệnh|
        ///       "medicineName": "Amoxicillin",   -> |Tên thuốc điều trị|
        ///       "disease": "Lở mồm long móng",   -> |Tên bệnh|
        ///       "symptom": "Sốt cao, bọt mép",   -> |Triệu chứng bệnh|
        ///       "status": "Đã điều trị khỏi" -> |Mô tả chi tiết|
        ///     },
        ///     {
        ///       "dateOfRecord": "2025-01-20T14:30:00", -> |Thời gian phát hiện bệnh|
        ///       "medicineName": "Cefalexin",     -> |Tên thuốc điều trị|
        ///       "disease": "Viêm phổi",          -> |Tên bệnh|
        ///       "symptom": "Khó thở, ho",        -> |Triệu chứng bệnh|
        ///       "status": "Đã điều trị khỏi" -> |Mô tả chi tiết|
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của gia súc cần xem lịch sử bệnh</param>
        /// <returns>Lịch sử bệnh của gia súc</returns>
        /// <response code="200">Thành công, trả về lịch sử bệnh</response>
        /// <response code="404">Không tìm thấy gia súc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-sickness-history/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockSicknessHistory>> GetLiverstockSicknessHistory(string id)
        {
            try
            {
                var data = await _livestockRepository.GetLivestockSicknessHistory(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLiverstockSicknessHistory)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy lịch sử tiêm phòng của gia súc
        /// </summary>
        /// <remarks>
        /// API này trả về lịch sử tiêm phòng và vaccine của một gia súc theo ID.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "livestockId": "LS001",              -> |ID của vật nuôi|
        ///   "inspectionCode": "B01-001",         -> |Mã kiểm định của vật nuôi|
        ///   "total": 2,                          -> |Tổng số lần tiêm phòng|
        ///   "items": [
        ///     {
        ///       "dateTime": "2025-03-05T10:00:00", -> |Thời gian tiêm phòng|
        ///       "vaccineId": "VAC001",           -> |ID của vaccine|
        ///       "vaccineName": "Vaccine LMLM type O", -> |Tên vaccine|
        ///       "description": "Tiêm phòng định kỳ" -> |Mô tả chi tiết|
        ///     },
        ///     {
        ///       "dateTime": "2025-01-18T11:30:00", -> |Thời gian tiêm phòng|
        ///       "vaccineId": "VAC002",           -> |ID của vaccine|
        ///       "vaccineName": "Vaccine tụ huyết trùng", -> |Tên vaccine|
        ///       "description": "Tiêm phòng khi nhập đàn" -> |Mô tả chi tiết|
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của gia súc cần xem lịch sử tiêm phòng</param>
        /// <returns>Lịch sử tiêm phòng của gia súc</returns>
        /// <response code="200">Thành công, trả về lịch sử tiêm phòng</response>
        /// <response code="404">Không tìm thấy gia súc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-vaccination-history/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockSicknessHistory>> GetLiverstockVaccinationHistory(string id)
        {
            try
            {
                var data = await _livestockRepository.GetLivestockVaccinationHistory(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLiverstockVaccinationHistory)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Xuất danh sách gia súc chưa có mã định danh ra Excel
        /// </summary>
        /// <remarks>
        /// API này tạo và trả về file Excel chứa danh sách gia súc chưa có mã định danh.
        /// File Excel bao gồm các thông tin cơ bản và mã QR để in tem dán cho gia súc.
        /// </remarks>
        /// <returns>File Excel chứa danh sách gia súc và mã QR</returns>
        /// <response code="200">Thành công, trả về file Excel</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("export-list-no-code-livestock-excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ExportListNoCodeLivestockExcel()
        {
            try
            {
                var fileBytes = await _livestockRepository.ExportListNoCodeLivestockExcel();
                return File(fileBytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "Livestock_QR.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ExportListNoCodeLivestockExcel)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPost("get-livestockId-by-inspectioncode-and-type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockSummary>> GetLivestockByInspectionCodeAndType(LivestockIdFindDTO model)
        {
            try
            {
                var data = await _livestockRepository.GetLiveStockIdByInspectionCodeAndType(model);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetLivestockByInspectionCodeAndType)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

    }
}

