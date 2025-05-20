using AutoMapper;
using Azure.Messaging;
using BusinessObjects;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.AutoMapperConfig;
using DataAccess.Repository.Interfaces;
using DataAccess.Repository.Services;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    /// <summary>
    /// API quản lý thông tin thuốc và vaccine trong hệ thống
    /// </summary>
    [Route("api/medicine-management")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý thuốc: thêm, sửa, xóa và tìm kiếm thông tin thuốc trong hệ thống")]
    public class MedicalController : BaseAPIController
    {
        private readonly IMedicalRepository _medicalRepository;
        private readonly ILogger<MedicalController> _logger;


        public MedicalController(IMedicalRepository medicalRepository, ILogger<MedicalController> logger)
        {
            _medicalRepository = medicalRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các loại thuốc
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả các loại thuốc có trong hệ thống, bao gồm:
        /// - VACCINE: Thuốc dùng để tiêm phòng ngừa bệnh
        /// - THUỐC_CHỮA_BỆNH: Thuốc dùng để điều trị bệnh
        /// - KHÁNG_SINH: Thuốc kháng sinh để diệt vi khuẩn
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     "VACCINE",           -> |Loại thuốc phòng bệnh|
        ///     "THUỐC_CHỮA_BỆNH",   -> |Loại thuốc điều trị|
        ///     "KHÁNG_SINH"         -> |Loại thuốc kháng sinh|
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách các loại thuốc</returns>
        /// <response code="200">Thành công, trả về danh sách loại thuốc</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-medicine-type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetListMedicineType()
        {
            try
            {
                var data = Enum.GetNames(typeof(medicine_type)).ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListMedicines)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin thuốc theo ID
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin chi tiết của một thuốc theo ID.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "MED001",                                    -> |ID duy nhất của thuốc|
        ///     "name": "Terramycin",                              -> |Tên thuốc|
        ///     "description": "Điều trị nhiễm khuẩn đường hô hấp", -> |Mô tả chi tiết về tác dụng thuốc|
        ///     "type": "KHÁNG_SINH",                              -> |Loại thuốc (VACCINE, THUỐC_CHỮA_BỆNH, KHÁNG_SINH)|
        ///     "createdAt": "2026-05-15T00:00:00"                 -> |Thời gian tạo thuốc trong hệ thống|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của thuốc cần xem thông tin</param>
        /// <returns>Thông tin chi tiết của thuốc</returns>
        /// <response code="200">Thành công, trả về thông tin thuốc</response>
        /// <response code="404">Không tìm thấy thuốc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-medicine-by-id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicineSummary>> GetById([FromRoute] string id)
        {
            try
            {
                var medicine = await _medicalRepository.GetByIdAsync(id);
                return GetSuccess(medicine);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetById)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách thuốc theo các tiêu chí lọc
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách thuốc theo các tiêu chí lọc như tên, loại thuốc.
        /// 
        /// Các tham số lọc:
        /// - keyword: Từ khóa tìm kiếm theo tên thuốc
        /// - Type: Loại thuốc (VACCINE, THUỐC_CHỮA_BỆNH, KHÁNG_SINH)
        /// - FromDate: Ngày bắt đầu khoảng thời gian tìm kiếm (định dạng yyyy-MM-dd)
        /// - ToDate: Ngày kết thúc khoảng thời gian tìm kiếm (định dạng yyyy-MM-dd)
        /// - pageSize: Số lượng bản ghi trên mỗi trang
        /// - pageNumber: Số trang
        /// 
        /// Ví dụ Query Parameters:
        /// ```
        /// ?keyword=Vaccine và Type=VACCINE và pageSize=10 và pageNumber=1
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "total": 2,                        -> |Tổng số thuốc thỏa mãn điều kiện lọc|
        ///     "items": [
        ///       {
        ///         "id": "MED002",                -> |ID của thuốc|
        ///         "name": "Vaccine LMLM type O", -> |Tên thuốc|
        ///         "description": "Phòng bệnh lở mồm long móng", -> |Mô tả chi tiết thuốc|
        ///         "type": "VACCINE",             -> |Loại thuốc|
        ///         "createdAt": "2026-08-20T00:00:00" -> |Thời gian tạo thuốc|
        ///       },
        ///       {
        ///         "id": "MED003",                -> |ID của thuốc|
        ///         "name": "Vaccine tụ huyết trùng", -> |Tên thuốc|
        ///         "description": "Phòng bệnh tụ huyết trùng", -> |Mô tả chi tiết thuốc|
        ///         "type": "VACCINE",             -> |Loại thuốc|
        ///         "createdAt": "2026-06-10T00:00:00" -> |Thời gian tạo thuốc|
        ///       }
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="query">Các tham số để lọc danh sách thuốc</param>
        /// <returns>Danh sách thuốc phù hợp với tiêu chí lọc</returns>
        /// <response code="200">Thành công, trả về danh sách thuốc</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-medicine")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListMedicine>> GetListMedicines([FromQuery] MedicinesFliter query)
        {
            try
            {
                var medicines = await _medicalRepository.GetListMedicineAsync(query);
                if (medicines == null)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(GetListMedicines)} List ia empty.");
                    return GetSuccess(null);
                }
                return GetSuccess(medicines);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListMedicines)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Tạo mới thuốc
        /// </summary>
        /// <remarks>
        /// API này thực hiện tạo mới một thuốc trong hệ thống.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Amoxicillin 15%",                     -> |Tên thuốc|
        ///   "description": "Điều trị nhiễm khuẩn, viêm phổi", -> |Mô tả chi tiết về tác dụng thuốc|
        ///   "type": "KHÁNG_SINH",                          -> |Loại thuốc (VACCINE, THUỐC_CHỮA_BỆNH, KHÁNG_SINH)|
        ///   "createdBy": "user-001"                        -> |ID người tạo thuốc|
        /// }
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "MED009",                              -> |ID duy nhất của thuốc|
        ///     "name": "Amoxicillin 15%",                   -> |Tên thuốc|
        ///     "description": "Điều trị nhiễm khuẩn, viêm phổi", -> |Mô tả chi tiết về tác dụng thuốc|
        ///     "type": "KHÁNG_SINH",                        -> |Loại thuốc (VACCINE, THUỐC_CHỮA_BỆNH, KHÁNG_SINH)|
        ///     "createdAt": "2023-03-15T08:30:00",          -> |Thời gian tạo thuốc|
        ///     "createdBy": "user-001",                     -> |ID người tạo thuốc|
        ///     "updatedAt": "2023-03-15T08:30:00",          -> |Thời gian cập nhật thuốc gần nhất|
        ///     "updatedBy": "user-001"                      -> |ID người cập nhật thuốc gần nhất|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="createMedicineDTO">Thông tin thuốc cần tạo</param>
        /// <returns>Thông tin thuốc sau khi tạo</returns>
        /// <response code="200">Thành công, trả về thông tin thuốc đã tạo</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("create-medicine")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicineDTO>> Create([FromBody] CreateMedicineDTO createMedicineDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Create)} ModelState not Valid");
                    return GetError("ModelState not Valid");
                }
                var medicineModels = await _medicalRepository.CreateAsync(createMedicineDTO);
                return SaveSuccess(medicineModels);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin thuốc
        /// </summary>
        /// <remarks>
        /// API này thực hiện cập nhật thông tin của một thuốc đã tồn tại trong hệ thống.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Amoxicillin 20%",                       -> |Tên mới của thuốc|
        ///   "description": "Điều trị nhiễm khuẩn đường hô hấp và tiêu hóa", -> |Mô tả chi tiết về tác dụng thuốc|
        ///   "type": "KHÁNG_SINH",                            -> |Loại thuốc (VACCINE, THUỐC_CHỮA_BỆNH, KHÁNG_SINH)|
        ///   "updatedBy": "user-002"                          -> |ID người cập nhật thuốc|
        /// }
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "MED009",                                -> |ID duy nhất của thuốc|
        ///     "name": "Amoxicillin 20%",                     -> |Tên đã cập nhật của thuốc|
        ///     "description": "Điều trị nhiễm khuẩn đường hô hấp và tiêu hóa", -> |Mô tả chi tiết đã cập nhật|
        ///     "type": "KHÁNG_SINH",                          -> |Loại thuốc (VACCINE, THUỐC_CHỮA_BỆNH, KHÁNG_SINH)|
        ///     "createdAt": "2023-03-15T08:30:00"             -> |Thời gian tạo thuốc ban đầu|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của thuốc cần cập nhật</param>
        /// <param name="updateMedicineDTO">Thông tin cập nhật</param>
        /// <returns>Thông tin thuốc sau khi cập nhật</returns>
        /// <response code="200">Thành công, trả về thông tin thuốc đã cập nhật</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy thuốc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut]
        [Route("update-medicine/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicineSummary>> Update([FromRoute] string id, [FromBody] UpdateMedicineDTO updateMedicineDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Update)} ModelState not Valid");
                    return GetError("ModelState not Valid");
                }
                var medicineModles = await _medicalRepository.UpdateAsync(id, updateMedicineDTO);
                return SaveSuccess(medicineModles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Update)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Xóa thuốc
        /// </summary>
        /// <remarks>
        /// API này thực hiện xóa một thuốc khỏi hệ thống theo ID cung cấp.
        /// Không thể xóa thuốc nếu thuốc đang được tham chiếu trong lịch sử bệnh hoặc lịch sử tiêm phòng.
        /// 
        /// Ví dụ Response khi thành công:
        /// ```json
        /// {
        ///   "data": "Xoá thành công!"     -> |Thông báo kết quả xóa thành công|
        /// }
        /// ```
        /// 
        /// Ví dụ Response khi lỗi:
        /// ```json
        /// {
        ///   "data": "Không thể xóa vì thuốc này đang được sử dụng trong hệ thống."     -> |Thông báo lỗi khi xóa|
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của thuốc cần xóa</param>
        /// <returns>Kết quả xóa thuốc</returns>
        /// <response code="200">Thành công, đã xóa thuốc</response>
        /// <response code="400">Không thể xóa vì thuốc đang được sử dụng</response>
        /// <response code="404">Không tìm thấy thuốc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpDelete]
        [Route("delete-medicine/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Delete([FromRoute] string id)
        {
            try
            {
                var medicineModles = await _medicalRepository.DeleteAsync(id);
                return SaveSuccess("Xoá thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Delete)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
        [HttpGet("get-list-medicine-by-disease/{diseaseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<MedicineDTO>>> GetListMedicineOfDisease([FromRoute] string diseaseId)
        {
            try
            {
                var data = await _medicalRepository.GetMedicineByDisease(diseaseId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListMedicineOfDisease)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
