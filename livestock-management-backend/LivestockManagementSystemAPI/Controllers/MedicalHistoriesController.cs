using AutoMapper;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    /// <summary>
    /// API quản lý lịch sử y tế của gia súc trong hệ thống
    /// </summary>
    [Route("api/medicine-histories")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý lịch sử y tế: xem các lần khám, điều trị và tiêm phòng của gia súc")]
    public class MedicalHistoriesController : BaseAPIController
    {
        private readonly IMedicalHistoriesRepository _medicalHistoriesRepository;
        private readonly ILogger<MedicalHistoriesController> _logger;
        private readonly ILivestockRepository _livestockRepository;

        public MedicalHistoriesController(ILivestockRepository livestockRepository, IMedicalHistoriesRepository medicalHistoriesRepository, ILogger<MedicalHistoriesController> logger)
        {
            _medicalHistoriesRepository = medicalHistoriesRepository;
            _logger = logger;
            _livestockRepository = livestockRepository;
        }

        /// <summary>
        /// Lấy lịch sử y tế của gia súc theo ID
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các lần điều trị y tế của gia súc, bao gồm thông tin về bệnh, thuốc và thời gian điều trị.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "diseaseName": "Lở mồm long móng",   -> |Tên bệnh được điều trị|
        ///       "medicineName": "Terramycin",        -> |Tên thuốc được sử dụng|
        ///       "createdAt": "2025-03-15T10:30:00"   -> |Thời gian bắt đầu điều trị|
        ///     },
        ///     {
        ///       "diseaseName": "Viêm phổi",          -> |Tên bệnh được điều trị|
        ///       "medicineName": "Amoxicillin 15%",   -> |Tên thuốc được sử dụng|
        ///       "createdAt": "2025-02-10T08:45:00"   -> |Thời gian bắt đầu điều trị|
        ///     }
        ///   ]
        /// }
        /// ```
        /// 
        /// Trường hợp gia súc chưa có lịch sử y tế, API sẽ trả về mảng rỗng:
        /// ```json
        /// {
        ///   "data": []
        /// }
        /// ```
        /// </remarks>
        /// <param name="livestockId">ID của gia súc cần xem lịch sử y tế</param>
        /// <returns>Danh sách lịch sử y tế của gia súc</returns>
        /// <response code="200">Thành công, trả về lịch sử y tế</response>
        /// <response code="404">Không tìm thấy gia súc với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-medical-histories-by-livestock-id/{livestockId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<MedicalHistoriesGeneral>>> GetById([FromRoute] string livestockId)
        {
            try
            {
                List<MedicalHistoriesGeneral> medicalHistoriesGeneral = await _medicalHistoriesRepository.GetMedicalHistoriesGeneralInfoById(livestockId);
                return GetSuccess(medicalHistoriesGeneral);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetById)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
