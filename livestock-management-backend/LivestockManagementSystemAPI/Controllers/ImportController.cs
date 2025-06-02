using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BusinessObjects.Constants.LmsConstants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Annotations;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/import-management")]
    [ApiController]
    //[Authorize] // Changed from AllowAnonymous to Authorize for security
    [AllowAnonymous]
    [SwaggerTag("Quản lý nhập gia súc: tạo, cập nhật, và theo dõi các lô nhập gia súc")]
    public class ImportController : BaseAPIController
    {
        private readonly IImportRepository _importRepository;
        private readonly ILogger<ImportController> _logger;
        public ImportController(IImportRepository importRepository, ILogger<ImportController> logger)
        {
            _importRepository = importRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các trạng thái của lô nhập
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả các trạng thái lô nhập có trong hệ thống, bao gồm:
        /// - CHỜ_NHẬP
        /// - ĐANG_NHẬP
        /// - ĐÃ_NHẬP
        /// - HOÀN_THÀNH
        /// - ĐÃ_HỦY
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     "ĐÃ_HỦY",
        ///     "CHỜ_NHẬP",
        ///     "ĐANG_NHẬP",
        ///     "ĐÃ_NHẬP",
        ///     "HOÀN_THÀNH"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách các trạng thái lô nhập</returns>
        /// <response code="200">Thành công, trả về danh sách trạng thái</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-import-batch-statuses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetListBatchImportStatuses()
        {
            try
            {
                var data = Enum.GetValues(typeof(batch_import_status))
                    .Cast<batch_import_status>()
                    .Where(x => x != batch_import_status.ĐÃ_NHẬP)
                    .Select(x => x.ToString())
                    .ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListBatchImportStatuses)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách lô nhập gia súc
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các lô nhập gia súc theo các tiêu chí lọc như khoảng thời gian, trạng thái.
        /// 
        /// Ví dụ Query Parameters:
        /// - fromDate: "2023-01-01"  -> |Thời gian bắt đầu lọc|
        /// - toDate: "2023-12-31"    -> |Thời gian kết thúc lọc|
        /// - status: "ĐANG_NHẬP"     -> |Trạng thái lô nhập cần lọc|
        /// - pageSize: 10            -> |Số lượng kết quả mỗi trang|
        /// - pageNumber: 1           -> |Trang hiện tại|
        /// - keyword: "Lô nhập"      -> |Từ khóa tìm kiếm|
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "total": 2,
        ///     "items": [
        ///       {
        ///         "id": "IMP001",
        ///         "name": "Lô nhập tháng 1/2023",
        ///         "estimatedQuantity": 50,
        ///         "importedQuantity": 42,
        ///         "expectedCompletionDate": "2023-01-31T00:00:00",
        ///         "completionDate": "2023-01-25T14:30:00",
        ///         "status": "HOÀN_THÀNH",
        ///         "createdBatchBy": "Nguyễn Văn A",
        ///         "createdBatchAt": "2023-01-10T08:00:00"
        ///       },
        ///       {
        ///         "id": "IMP002",
        ///         "name": "Lô nhập tháng 2/2023",
        ///         "estimatedQuantity": 30,
        ///         "importedQuantity": 10,
        ///         "expectedCompletionDate": "2023-02-28T00:00:00",
        ///         "completionDate": null,
        ///         "status": "ĐANG_NHẬP",
        ///         "createdBatchBy": "Nguyễn Thị B",
        ///         "createdBatchAt": "2023-02-05T09:15:00"
        ///       }
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="filter">Các tham số lọc cho danh sách lô nhập</param>
        /// <returns>Danh sách lô nhập gia súc phù hợp với tiêu chí lọc</returns>
        /// <response code="200">Thành công, trả về danh sách lô nhập</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-import-batches")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListImportBatches>> GetListImportBatches([FromQuery] ListImportBatchesFilter filter)
        {
            try
            {
                if (filter == null)
                    filter = new ListImportBatchesFilter();

                var data = await _importRepository.GetListImportBatches(filter);

                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListImportBatches)} : {ex.Message}");
                return GetError("Không thể lấy danh sách lô nhập: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của lô nhập gia súc
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin chi tiết của một lô nhập gia súc, bao gồm thông tin chung và danh sách gia súc đã nhập.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "IMP001",
        ///     "name": "Lô nhập tháng 1/2023",
        ///     "estimatedQuantity": 50,
        ///     "importedQuantity": 42,
        ///     "expectedCompletionDate": "2023-01-31T00:00:00",
        ///     "completionDate": "2023-01-25T14:30:00",
        ///     "status": "HOÀN_THÀNH",
        ///     "createdBatchBy": "Nguyễn Văn A",
        ///     "createdBatchAt": "2023-01-10T08:00:00",
        ///     "originLocation": "Trang trại ABC, Lâm Đồng",
        ///     "importToBarn": "Chuồng A1",
        ///     "createdBy": "Nguyễn Văn A",
        ///     "createdAt": "2023-01-10T08:00:00",
        ///     "listImportedLivestocks": {
        ///       "total": 2,
        ///       "items": [
        ///         {
        ///           "id": "LS001",
        ///           "inspectionCode": "B01-001",
        ///           "specieId": "SP001",
        ///           "specieName": "Bò sữa",
        ///           "specieType": "BÒ",
        ///           "createdAt": "2023-01-15T10:30:00",
        ///           "importedBy": "Nguyễn Văn A",
        ///           "importedDate": "2023-01-15T10:30:00",
        ///           "weightImport": 450.5,
        ///           "status": "ĐÃ_NHẬP"
        ///         },
        ///         {
        ///           "id": "LS002",
        ///           "inspectionCode": "B01-002",
        ///           "specieId": "SP001",
        ///           "specieName": "Bò sữa",
        ///           "specieType": "BÒ",
        ///           "createdAt": "2023-01-16T09:45:00",
        ///           "importedBy": "Nguyễn Văn A",
        ///           "importedDate": "2023-01-16T09:45:00",
        ///           "weightImport": 420.3,
        ///           "status": "ĐÃ_NHẬP"
        ///         }
        ///       ]
        ///     }
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của lô nhập cần xem chi tiết</param>
        /// <returns>Thông tin chi tiết của lô nhập và danh sách gia súc đã nhập</returns>
        /// <response code="200">Thành công, trả về thông tin chi tiết lô nhập</response>
        /// <response code="404">Không tìm thấy lô nhập với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-import-batch-details/{batchImportId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ImportBatchDetails>> GetImportBatchDetails([FromRoute] string batchImportId)
        {
            try
            {
                var data = await _importRepository.GetImportBatchDetails(batchImportId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{nameof(ImportController)}] Lỗi khi lấy import batch: " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Hoàn thành lô nhập gia súc
        /// </summary>
        /// <remarks>
        /// API này đánh dấu một lô nhập gia súc là đã hoàn thành.
        /// Chỉ những lô nhập có trạng thái là "CHỜ_NHẬP" hoặc "ĐANG_NHẬP" mới có thể được hoàn thành.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "id": "IMP001",      -> |ID của lô nhập cần hoàn thành|
        ///   "requestedBy": "user123"  -> |ID của người yêu cầu hoàn thành|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": true
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "data": "Không thể hoàn thành lô nhập đã hủy"
        /// }
        /// ```
        /// </remarks>
        /// <param name="status">Thông tin hoàn thành lô nhập (ID lô nhập và người yêu cầu)</param>
        /// <returns>Kết quả hoàn thành lô nhập</returns>
        /// <response code="200">Thành công, đã hoàn thành lô nhập</response>
        /// <response code="400">Không thể hoàn thành do trạng thái không phù hợp</response>
        /// <response code="404">Không tìm thấy lô nhập với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("success")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Complete([FromBody] BatchImportStatus status)
        {
            try
            {
                var changeBatchImportStatusModel = await _importRepository.StatusComplete(status);
                return SaveSuccess(changeBatchImportStatusModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Complete)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Hủy lô nhập gia súc
        /// </summary>
        /// <remarks>
        /// API này đánh dấu một lô nhập gia súc là đã hủy.
        /// Chỉ những lô nhập có trạng thái là "CHỜ_NHẬP" hoặc "ĐANG_NHẬP" mới có thể bị hủy.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "id": "IMP001",      -> |ID của lô nhập cần hủy|
        ///   "requestedBy": "user123"  -> |ID của người yêu cầu hủy|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "data": true
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "data": "Không thể hủy lô nhập đã hoàn thành"
        /// }
        /// ```
        /// </remarks>
        /// <param name="status">Thông tin hủy lô nhập (ID lô nhập và người yêu cầu)</param>
        /// <returns>Kết quả hủy lô nhập</returns>
        /// <response code="200">Thành công, đã hủy lô nhập</response>
        /// <response code="400">Không thể hủy do trạng thái không phù hợp</response>
        /// <response code="404">Không tìm thấy lô nhập với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Cancel([FromBody] BatchImportStatus status)
        {
            try
            {
                var changeBatchImportStatusModel = await _importRepository.StatusCancel(status);
                return SaveSuccess(changeBatchImportStatusModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Cancel)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Tạo mới lô nhập gia súc
        /// </summary>
        /// <remarks>
        /// API này thực hiện tạo mới một lô nhập gia súc với thông tin cung cấp.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Lô nhập tháng 3/2023",                -> |Tên lô nhập|
        ///   "estimatedQuantity": 40,                        -> |Số lượng gia súc dự kiến|
        ///   "expectedCompletionDate": "2023-03-31T00:00:00", -> |Ngày hoàn thành dự kiến|
        ///   "originLocation": "Trang trại XYZ, Đắk Lắk",    -> |Vị trí xuất xứ|
        ///   "barnId": "BARN001",                           -> |Mã chuồng nhập vào|
        ///   "createdBy": "user123"                          -> |ID người tạo|
        /// }
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "IMP003",
        ///     "name": "Lô nhập tháng 3/2023",
        ///     "estimatedQuantity": 40,
        ///     "expectedCompletionDate": "2023-03-31T00:00:00",
        ///     "originLocation": "Trang trại XYZ, Đắk Lắk",
        ///     "barnId": "BARN001",
        ///     "status": "CHỜ_NHẬP",
        ///     "createdBy": "user123",
        ///     "createdAt": "2023-03-01T10:00:00"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="batchImportCreate">Thông tin lô nhập cần tạo</param>
        /// <returns>Thông tin lô nhập sau khi tạo</returns>
        /// <response code="200">Thành công, đã tạo lô nhập</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("create-batch-import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BatchImport>> CreateBatchImport([FromBody] BatchImportCreate batchImportCreate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(CreateBatchImport)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var batchImport = await _importRepository.CreateBatchImport(batchImportCreate);
                return SaveSuccess(batchImport);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CreateBatchImport)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin lô nhập gia súc
        /// </summary>
        /// <remarks>
        /// API này thực hiện cập nhật thông tin của một lô nhập gia súc đã tồn tại.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "id": "IMP003",                                -> |ID của lô nhập cần cập nhật|
        ///   "name": "Lô nhập tháng 3/2023 - Cập nhật",     -> |Tên lô nhập mới|
        ///   "estimatedQuantity": 45,                        -> |Số lượng gia súc dự kiến mới|
        ///   "expectedCompletionDate": "2023-04-15T00:00:00", -> |Ngày hoàn thành dự kiến mới|
        ///   "originLocation": "Trang trại ABC, Đắk Lắk",    -> |Vị trí xuất xứ mới|
        ///   "barnId": "BARN002",                           -> |Mã chuồng nhập vào mới|
        ///   "updatedBy": "user123"                          -> |ID người cập nhật|
        /// }
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "IMP003",
        ///     "name": "Lô nhập tháng 3/2023 - Cập nhật",
        ///     "estimatedQuantity": 45,
        ///     "expectedCompletionDate": "2023-04-15T00:00:00",
        ///     "originLocation": "Trang trại ABC, Đắk Lắk",
        ///     "barnId": "BARN002",
        ///     "status": "CHỜ_NHẬP",
        ///     "updatedBy": "user123",
        ///     "updatedAt": "2023-03-05T14:30:00"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="batchImportUpdate">Thông tin cập nhật lô nhập</param>
        /// <returns>Thông tin lô nhập sau khi cập nhật</returns>
        /// <response code="200">Thành công, đã cập nhật lô nhập</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy lô nhập với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("update-batch-import/{batchImportId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BatchImport>> UpdateBatchImport([FromRoute] string batchImportId, [FromBody] BatchImportUpdate batchImportUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(UpdateBatchImport)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var batchImport = await _importRepository.UpdateBatchImportAsync(batchImportId, batchImportUpdate);
                return SaveSuccess(batchImport);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateBatchImport)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }


        /// <summary>
        /// Xóa gia súc khỏi lô nhập
        /// </summary>
        /// <remarks>
        /// API này thực hiện xóa một gia súc ra khỏi lô nhập dựa trên ID của chi tiết lô nhập.
        /// Chỉ có thể xóa gia súc từ những lô nhập chưa hoàn thành hoặc chưa được nhập xong.
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
        ///   "data": "Lô nhập này đã hoàn thành hoặc đã nhập xong hoặc không tồn tại"
        /// }
        /// ```
        /// </remarks>
        /// <param name="importDetailId">ID của chi tiết trong 1 lô nhập cần xóa</param>
        /// <returns>Thông tin chi tiết nhập đã xóa</returns>
        /// <response code="200">Thành công, đã xóa gia súc khỏi lô nhập</response>
        /// <response code="400">Không thể xóa do lô nhập đã hoàn thành hoặc đã nhập xong</response>
        /// <response code="404">Không tìm thấy chi tiết nhập với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpDelete("delete-livestock-from-batch-import/{batchImportDetailsId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeleteLivestockFromBatchImport([FromRoute] string batchImportDetailsId)
        {
            try
            {
                var result = await _importRepository.DeleteLivestockFromBatchImport(batchImportDetailsId);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(DeleteLivestockFromBatchImport)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpGet("get-batchimport-livestock-info/{batchImportDetailsId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockBatchImportInfo>> GetBatchImportLiverstockInfo([FromRoute] string batchImportDetailsId)
        {
            try
            {
                var data = await _importRepository.GetLivestockInfoInBatchImport(batchImportDetailsId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetBatchImportLiverstockInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPut("set-livestock-dead/{batchImportDetailsId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> SetivestockAsDead([FromRoute] string batchImportDetailsId, string requestedBy)
        {
            try
            {
                var data = await _importRepository.SetLivestockDead(batchImportDetailsId, requestedBy);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(SetivestockAsDead)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPut("add-livestock-to-batchimport/{batchimportId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockBatchImportInfo>> AddLivestockToBatchImportDetails([FromRoute] string batchimportId, [FromBody] AddImportLivestockDTO livestockAddModel)
        {
            try
            {
                var data = await _importRepository.AddLivestockToDetails(batchimportId, livestockAddModel);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddLivestockToBatchImportDetails)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPut("update-livestock-in-batchimport/{batchimportDetailsId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockBatchImportInfo>> UpdateLivestockInBatchImportDetails([FromRoute] string batchimportDetailsId, [FromBody] UpdateImportLivestockDTO livestockAddModel)
        {
            try
            {
                var data = await _importRepository.UpdateLivestockInDetails(batchimportDetailsId, livestockAddModel);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateLivestockInBatchImportDetails)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpGet("get-list-pinned-batchimport/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListPinnedImportBatches>> GetListPinnedImportBatch([FromRoute] string userId)
        {
            try
            {
                var data = await _importRepository.GetListPinnedBatcImport(userId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListPinnedImportBatch)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-list-overdue-batchimport")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListOverDueImportBatches>> GetListOverDueImportBatch()
        {
            try
            {
                var data = await _importRepository.GetListOverDueBatchImport();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListOverDueImportBatch)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-list-neardue-batchimport/{num}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListNearDueImportBatches>> GetListNearDueImportBatch([FromRoute] int num)
        {
            try
            {
                var data = await _importRepository.GetListNearDueBatchImport(num);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListNearDueImportBatch)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-list-missing-batchimport")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListMissingImportBatches>> GetListMissingImportBatch()
        {
            try
            {
                var data = await _importRepository.GetListMissingBatchImport();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListMissingImportBatch)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-list-upcoming-batchimport/{num}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListUpcomingImportBatches>> GetListUpcomingImportBatch([FromRoute] int num)
        {
            try
            {
                var data = await _importRepository.GetListUpcomingBatchImport(num);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListUpcomingImportBatch)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPost("add-to-pinned-importbatch/{batchImportId}")]
        public async Task<ActionResult<bool>> AddToPinnedImportBatch([FromRoute] string batchImportId, [FromQuery] string? requestedBy)
        {
            try
            {
                var batchImport = await _importRepository.AddToPinImportBatch(batchImportId, requestedBy);
                return SaveSuccess(batchImport);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddToPinnedImportBatch)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
        
        [HttpDelete("remove-from-pinned-batch-import/{pinnedBatchImportId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> RemoveFromPinnedImportBatch([FromRoute] string pinnedBatchImportId, string requestedBy)
        {
            try
            {
                var result = await _importRepository.RemoveFromPinImportBatch(pinnedBatchImportId, requestedBy);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(RemoveFromPinnedImportBatch)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }


        //[HttpGet("get-batch-import-scan/{batchImportId}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<BatchImportScanDTO>> GetBatchImportScan([FromRoute] string batchImportId)
        //{
        //    try
        //    {
        //        var data = await _importRepository.GetBatchImportScanDetails(batchImportId);
        //        return GetSuccess(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"[{this.GetType().Name}]/{nameof(GetBatchImportScan)} " + ex.Message);
        //        return GetError(ex.Message);
        //    }
        //}

        [HttpGet("get-list-search-history/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListSearchHistory>> GetListBatchImportSearchHistory([FromRoute] string userId)
        {
            try
            {
                var data = await _importRepository.GetListSearchHistory(userId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListBatchImportSearchHistory)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPost("confirm-imported/{livestockId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ConfirmImportedLivestock([FromRoute] string livestockId, string requestedBy)
        {
            try
            {
                var data = await _importRepository.ConfirmImportedToBarn(livestockId, requestedBy);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ConfirmImportedLivestock)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpGet("get-batchimport-livestock-scan-info/{livestockId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockBatchImportScanInfo>> GetBatchImportLiverstockScanInfo([FromRoute] string livestockId)
        {
            try
            {
                var data = await _importRepository.GetLivestockScanInfo(livestockId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetBatchImportLiverstockScanInfo)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPost("confirm-livestock-for-meat-sale/{livestockId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ConfirmLivestockForMeatSale([FromRoute] string livestockId, string requestedBy)
        {
            try
            {
                var data = await _importRepository.SetForSaleLivestock(livestockId, requestedBy);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ConfirmLivestockForMeatSale)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPut("confirm-replace-livestock-to-batch-import/{batchimportId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockBatchImportScanInfo>> ConfirmreplaceLivestockToBatchImport([FromRoute] string batchimportId, [FromBody] AddImportLivestockDTO livestockAddModel)
        {
            try
            {
                var data = await _importRepository.ConfrimReplaceDeadLiveStock(batchimportId, livestockAddModel);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ConfirmreplaceLivestockToBatchImport)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpGet("get-list-import-for-admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListSearchHistory>> GetListBatchImportForAdmin([FromQuery] ListImportBatchesFilter filter)
        {
            try
            {
                var data = await _importRepository.GetListBatchImportForAdmin(filter);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListBatchImportForAdmin)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
