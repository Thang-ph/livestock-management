using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using BusinessObjects.Models;
using ExcelDataReader;
using System.Text;
using BusinessObjects.ConfigModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static BusinessObjects.Constants.LmsConstants;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;
using static BusinessObjects.Dtos.BatchExportDTO;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/export-management")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý lô xuất: tìm kiếm, xem chi tiết danh sách khách hàng của lô xuất")]
    public class BatchExportController : BaseAPIController
    {
        private readonly IBatchExportRepository _batchExportRepository;
        private readonly ILogger<BatchExportController> _logger;

        public BatchExportController(IBatchExportRepository batchExportRepository, ILogger<BatchExportController> logger)
        {
            _batchExportRepository = batchExportRepository;
            _logger = logger;
        }

        /// <summary>
        /// lấy danh sách khách hàng theo ID gói thầu
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách thông tin khách hàng dựa trên ID gói thầu và bộ lọc
        /// 
        /// Sample request:
        /// 
        /// POST /api/export-management/get-list-customers/{id}
        /// ```json
        /// {
        ///     "keyword": "ten khach hang", -> | Từ khóa tìm kiếm theo tên khách hàng |
        ///     "fromDate": "2025-04-01T00:00:00Z", -> | Ngày bắt đầu lọc (định dạng ISO) |
        ///     "toDate": "2025-04-30T23:59:59Z", -> | Ngày kết thúc lọc (định dạng ISO) |
        ///     "status": "CHỜ_BÀN_GIAO", -> | Trạng thái xuất lô (CHỜ_BÀN_GIAO, ĐÃ_BÀN_GIAO, ĐÃ_HỦY) |
        ///     "skip": 0, -> | Số bản ghi bỏ qua |
        ///     "take": 10 -> | Số bản ghi lấy |
        /// }
        /// ```
        /// Sample response:
        /// ```json
        /// {
        ///     "items": [
        ///         {
        ///             "id": "abc123", -> | ID của bản ghi xuất lô |
        ///             "customerName": "Nguyễn Văn A", -> | Tên khách hàng |
        ///             "customerPhone": "0987654321", -> | Số điện thoại khách hàng |
        ///             "customerAddress": "Hà Nội", -> | Địa chỉ khách hàng |
        ///             "customerNote": "Ghi chú", -> | Ghi chú về khách hàng |
        ///             "total": 10, -> | Tổng số gia súc cần bàn giao |
        ///             "remaining": 5, -> | Số gia súc còn lại chưa bàn giao |
        ///             "status": "CHỜ_BÀN_GIAO", -> | Trạng thái xuất lô |
        ///             "createdAt": "2025-04-15T10:30:00Z" -> | Thời gian tạo bản ghi |
        ///         }
        ///     ],
        ///     "total": 1 -> | Tổng số bản ghi thỏa mãn điều kiện lọc |
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của gói thầu</param>
        /// <param name="filter">Bộ lọc tìm kiếm khách hàng</param>
        /// <returns>Danh sách thông tin khách hàng</returns>
        [HttpPost("get-list-customers/{id}")]
        public async Task<ActionResult<ListCustomers>> GetListCustomersById([FromRoute] string id, [FromBody] CustomersFliter filter)
        {
            try
            {
                var batchModels = await _batchExportRepository.GetListCustomers(id, filter);
                if (batchModels == null)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(GetListCustomersById)} Danh sách bàn giao trống.");
                    return GetSuccess(null);
                }
                return GetSuccess(batchModels);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{nameof(BatchExportController)}] " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// lấy danh sách trạng thái lô xuất
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các trạng thái có thể có của việc xuất lô
        /// 
        /// ví dụ request:
        /// ```
        /// GET /api/export-management/get-list-batch-export-statuses
        /// ```
        /// ví dụ response:
        /// ```json
        /// [
        ///     "CHỜ_BÀN_GIAO", -> | Trạng thái chờ bàn giao gia súc cho khách hàng |
        ///     "ĐÃ_BÀN_GIAO", -> | Trạng thái đã bàn giao gia súc cho khách hàng |
        ///     "ĐÃ_HỦY" -> | Trạng thái đã hủy việc bàn giao |
        /// ]
        /// ```
        /// </remarks>
        /// <returns>Danh sách các trạng thái xuất lô</returns>
        [HttpGet("get-list-batch-export-statuses")]
        public async Task<ActionResult<IEnumerable<string>>> GetListBatchExportStatuses()
        {
            try
            {
                var data = Enum.GetNames(typeof(batch_export_status)).ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListBatchExportStatuses)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        //[HttpPost("upload/{procurementId}")]
        //public async Task<IActionResult> UploadExcel(string procurementId,[FromForm] string requestedBy, IFormFile file)
        //{
        //    try
        //    {
        //        await _batchExportRepository.ImportCustomer(procurementId, requestedBy, file);
        //        return GetSuccess("Thanh Cong");
        //    }
        //    catch (Exception ex)
        //    {
        //        return GetError(ex.Message);
        //    }
        //}
        [HttpDelete("delete-customer-in-batch-export/{batchExportID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeleteCustomerFromBatchExport([FromRoute] string batchExportID)
        {
            try
            {
                var result = await _batchExportRepository.DeleteCustomerFromBatchExport(batchExportID);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(DeleteCustomerFromBatchExport)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpPut("update-customer-in-batch-export/{batchExportId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BatchExport>> UpdateCustomerFromBatchExport([FromRoute]string batchExportId,[FromBody] UpdateCustomerBatchExportDTO updateCustomerBatchExportDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(UpdateCustomerFromBatchExport)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var result = await _batchExportRepository.UpdateCustomerFromBatchExport(batchExportId,updateCustomerBatchExportDTO);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateCustomerFromBatchExport)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpPost("add-customer-in-batch-export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BatchExport>> AddCustomerFromBatchExport([FromBody] AddCustomerBatchExportDTO addCustomerBatchExportDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(AddCustomerFromBatchExport)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var result = await _batchExportRepository.AddCustomerFromBatchExport(addCustomerBatchExportDTO);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddCustomerFromBatchExport)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("can-add-customer-in-batch-export/{procurementId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CanAddCustomerInBatchExport([FromRoute] string procurementId)
        {
            try
            {
                var result = await _batchExportRepository.CanAddCustomerInBatchExport(procurementId);
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CanAddCustomerInBatchExport)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpDelete("delete-livestock-from-batch-export-detail/{batchExportDetailId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeleteLivestockFromBatchExportDetail([FromRoute] string batchExportDetailId)
        {
            try
            {
                var result = await _batchExportRepository.DeleteLivestockFromBatchExportDetail(batchExportDetailId);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(DeleteLivestockFromBatchExportDetail)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpPost("add-livestock-to-batch-export-detail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> AddLivestockToBatchExportDetail( [FromBody] BatchExportDetailAddDTO batchExportDetailAdd)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(AddLivestockToBatchExportDetail)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var result = await _batchExportRepository.AddLivestockToBatchExportDetail(batchExportDetailAdd);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddLivestockToBatchExportDetail)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpPost("add-livestock-to-batch-export-detail-by-inspection-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> AddLivestockToBatchExportDetailByInspectionCode([FromBody] BatchExportDetailAddDTOByInspectionCode batchExportDetailAddDTOByInspectionCode)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(AddLivestockToBatchExportDetailByInspectionCode)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var result = await _batchExportRepository.AddLivestockToBatchExportDetailByInspectionCode(batchExportDetailAddDTOByInspectionCode);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddLivestockToBatchExportDetailByInspectionCode)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpPut("change-livestock-to-batch-export-detail/{batchExportDetailId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BatchExportDetail>> ChangeLivestockToBatchExportDetail([FromRoute] string batchExportDetailId,[FromBody] BatchExportDetailChangeDTO batchExportDetailChangeDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(ChangeLivestockToBatchExportDetail)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var result = await _batchExportRepository.ChangeLivestockToBatchExportDetail(batchExportDetailId,batchExportDetailChangeDTO);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ChangeLivestockToBatchExportDetail)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Body la user ID update
        /// </summary>

        [HttpPut("confirm-handover-batch-export-detail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ConfirmHandoverBatchExportDetail([FromBody] BatchExportHandover batchExportHandover)
        {
            try
            {
                var result = await _batchExportRepository.ConfirmHandoverBatchExportDetail(batchExportHandover.livestockId, batchExportHandover.UpdatedBy);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ConfirmHandoverBatchExportDetail)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpPut("confirm-handover-batch-export-detail-by-inspection-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ConfirmHandoverBatchExportDetailByInspectionCode([FromBody] BatchExportHandoverByInspection batchExportHandoverByInspection)
        {
            try
            {
                var result = await _batchExportRepository.ConfirmHandoverBatchExportDetailByInspectionCode(batchExportHandoverByInspection.inspectionCode, batchExportHandoverByInspection.specieType, batchExportHandoverByInspection.UpdatedBy);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ConfirmHandoverBatchExportDetailByInspectionCode)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpPut("update-batch-export-detail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BatchExportDetail>> UpdateBatchExportDetail([FromBody] BatchExportDetailUpdateDTO batchExportDetailUpdateDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(UpdateBatchExportDetail)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var result = await _batchExportRepository.UpdateBatchExportDetail(batchExportDetailUpdateDTO);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateBatchExportDetail)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
        [HttpGet("can-change-livestock-in-batch-export-detail/{batchExportDetailId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CanChangeLivestockInBatchExportDetail([FromRoute] string batchExportDetailId)
        {
            try
            {
                var result = await _batchExportRepository.CanChangeLivestockInBatchExportDetail(batchExportDetailId);
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CanChangeLivestockInBatchExportDetail)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
