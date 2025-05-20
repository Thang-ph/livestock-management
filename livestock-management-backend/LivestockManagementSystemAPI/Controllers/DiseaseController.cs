using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    /// <summary>
    /// API quản lý thông tin bệnh của hệ thống chăn nuôi
    /// </summary>
    [Route("api/disease-management")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý thông tin bệnh: thêm, sửa, xóa và tìm kiếm thông tin bệnh trong hệ thống")]
    public class DiseaseController : BaseAPIController
    {
        private readonly IDiseaseRepository _diseaseRepository;
        private readonly ILogger<DiseaseController> _logger;


        public DiseaseController(IDiseaseRepository diseaseRepository, ILogger<DiseaseController> logger)
        {
            _diseaseRepository = diseaseRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các trạng thái của các loại bệnh
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả các trạng thái loại bệnh có trong hệ thống, bao gồm:
        /// - TRUYỀN_NHIỄM_NGUY_HIỂM
        /// - TRUYỀN_NHIỄM
        /// - KHÔNG_TRUYỀN_NHIỄM
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///  "statusCode": 200,
        /// "success": true,
        ///"data": [
        ///   "TRUYỀN_NHIỄM_NGUY_HIỂM",
        ///   "TRUYỀN_NHIỄM",
        ///   "KHÔNG_TRUYỀN_NHIỄM"
        /// ],
        /// "errors": null,
        /// "message": "Get Success"
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách các loại bệnh</returns>
        /// <response code="200">Thành công, trả về danh sách loại bệnh</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-disease-type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetListDiseaseType()
        {
            try
            {
                var data = Enum.GetNames(typeof(disease_type)).ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListDiseaseType)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách bệnh
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các bệnh có trong hệ thống.
        /// Có thể tìm kiếm bệnh bằng từ khóa tùy chọn.
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
        ///         "id": "0jk1pabnmv83fghijklmnjq7r",
        ///         "name": "Lao bò"
        ///       },
        ///       {
        ///         "id": "1fb9pyzsw120ynzffsvcgkejl",
        ///         "name": "Lở mồm long móng"
        ///       }
        ///     ]
        ///   },
        ///   "errors": null,
        ///   "message": "Get Success"
        /// }
        /// ```
        /// </remarks>
        /// <param name="keyword">Từ khóa tìm kiếm (tùy chọn)</param>
        /// <returns>Danh sách bệnh tìm được</returns>
        /// <response code="200">Thành công, trả về danh sách bệnh</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-diseases")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListDiseases>> GetListDiseases([FromQuery] string? keyword)
        {
            try
            {
                var data = await _diseaseRepository.GetListDiseases(keyword);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListDiseases)} Get list diseases failed: " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin bệnh
        /// </summary>
        /// <remarks>
        /// API này thực hiện cập nhật thông tin của một bệnh đã tồn tại trong hệ thống.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Lở mồm long móng loại A",
        ///   "symptom": "Sốt cao, lở loét miệng và móng",
        ///   "description": "Bệnh truyền nhiễm nguy hiểm ở trâu bò",
        ///   "type": "TRUYỀN_NHIỄM_NGUY_HIỂM",
        ///   "requestedBy": "jer94jfne90nw93", -> |id của người dùng đang thực hiện cập nhật|
        /// }
        /// ```
        /// 
        /// Ví dụ Response thành công:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "message": "Thao tác thành công",
        ///   "data": {
        ///     "id": "1fb9pyzsw120ynzffsvcgkejl",
        ///     "name": "Lở mồm long móng loại A",
        ///     "symptom": "Sốt cao, lở loét miệng và móng",
        ///     "description": "Bệnh truyền nhiễm nguy hiểm ở trâu bò",
        ///     "type": "TRUYỀN_NHIỄM_NGUY_HIỂM"
        ///   },
        ///   "errors": [],
        ///   "message": "Save Success"
        /// }
        /// ```
        /// 
        /// Ví dụ Response thất bại:
        /// ```json
        /// {
        ///   "statusCode": 400,
        ///   "success": false,
        ///   "data": "Không tìm thấy bệnh",
        ///   "errors": null,
        ///   "message": "Save Data Failed"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của bệnh cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật</param>
        /// <returns>Thông tin bệnh sau khi cập nhật</returns>
        /// <response code="200">Thành công, trả về thông tin bệnh đã cập nhật</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy bệnh với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("update-diseases/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DiseaseDTO>> UpdateDisease([FromRoute] string id, [FromBody] DiseaseUpdateDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(UpdateDisease)} ModelState not Valid");
                    return GetError("ModelState not Valid");
                }
                var data = await _diseaseRepository.UpdateDisease(id, model);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateDisease)}  Update diseases failed: " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Xóa bệnh
        /// </summary>
        /// <remarks>
        /// API này thực hiện xóa một bệnh khỏi hệ thống theo ID cung cấp.
        /// Không thể xóa bệnh nếu bệnh đang được tham chiếu trong lịch sử bệnh hoặc đang được liên kết với thuốc.
        /// 
        /// Ví dụ Response khi thành công:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "data": "Xoá thành công!",
        ///   "errors": null,
        ///   "message": "Save Success"
        /// }
        /// ```
        /// 
        /// Ví dụ Response khi không thể xóa do ràng buộc:
        /// ```json
        /// {
        ///   "statusCode": 400,
        ///   "success": false,
        ///   "data": "Không thể xoá bệnh này vì đang được sử dụng trong hệ thống",
        ///   "errors": null,
        ///   "message": "Save Data Failed"
        /// }
        /// ```
        /// 
        /// Ví dụ Response khi không tìm thấy bệnh:
        /// ```json
        /// {
        ///   "statusCode": 404,
        ///   "success": false,
        ///   "data": "Không tìm thấy bệnh",
        ///   "errors": null,
        ///   "message": "Save Data Failed"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của bệnh cần xóa</param>
        /// <returns>Kết quả xóa bệnh</returns>
        /// <response code="200">Thành công, đã xóa bệnh</response>
        /// <response code="400">Không thể xóa vì bệnh đang được sử dụng trong hệ thống</response>
        /// <response code="404">Không tìm thấy bệnh với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpDelete("delete-disease/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeleteDisease([FromRoute] string id)
        {
            try
            {
                var data = await _diseaseRepository.DeleteDisease(id);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(DeleteDisease)}  Delete diseases failed: " + ex.Message);
                return SaveError(ex.Message);
            }
        }
    }
}
