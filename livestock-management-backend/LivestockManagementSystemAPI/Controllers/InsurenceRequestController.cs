using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/insurence-request")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý các đơn bảo hành: tìm kiếm, thêm, ....")]
    public class InsurenceRequestController : BaseAPIController
    {
        private readonly IInsuranceRequestRepository _insurenceRepository;
        private readonly ILogger<LivestockController> _logger;

        public InsurenceRequestController(IInsuranceRequestRepository insurenceRepository, ILogger<LivestockController> logger)
        {
            _insurenceRepository = insurenceRepository;
            _logger = logger;
           
        }

        /// <summary>
        /// Tạo mới đơn bảo hành (gửi yêu cầu bảo hành)
        /// </summary>
        [HttpPost("create-insurence-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateInsurenceDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Create)} ModelState not Valid");
                    return GetError("ModelState not Valid");
                }
                var codeRangeModel = await _insurenceRepository.CreateInsurenceRequest(createDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy chi tiết một đơn bảo hành theo id
        /// </summary>
        [HttpGet("get-insurence-request/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInfoById([FromRoute] String id)
        {
            try
            {
                
                var codeRangeModel = await _insurenceRepository.GetInsurenceRequestInfo(id);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }


        /// <summary>
        /// Lấy danh sách bảo hành
        /// </summary>
        [HttpGet("get-list-insurence-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListInsurance([FromQuery] InsurenceFilter filter)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.GetInsurenceList(filter);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách trạng thái của bảo hành
        /// </summary>
        [HttpGet("get-list-insurence-request-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListInsuranceStatus()
        {
            try
            {

                var codeRangeModel =  _insurenceRepository.GetAllStatusInsurence();
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách của bảo hành theo group
        /// </summary>
        [HttpGet("get-list-insurence-request-overview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListInsuranceOverview()
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.GetPendingOverviewList();
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Thay đổi trạng thái của một đơn bảo hành
        /// </summary>
        [HttpPut("update-status-insurence-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatusInsuranceRequest([FromBody] ChangeStatusInsuranceDto insuranceDto)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.ChangeStatusInsurance(insuranceDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Xóa vật nuôi của đơn bảo hành
        /// </summary>
        [HttpPut("delete-livestock-insurence-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteLivestockInsurance([FromBody] RemoveInsuranceLivestockDto removeDto)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.RemoveNewLivestockInsuranceRequest(removeDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật vật nuôi của đơn bảo hành
        /// </summary>
        [HttpPut("update-livestock-insurence-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLivestockInsurance([FromBody] UpdateInsuranceLivestockDto insuranceDto)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.UpdateNewLivestockInsurenceRequest(insuranceDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Chấp thuận đơn bảo hành
        /// </summary>
        [HttpPut("approve-livestock-insurence-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveLivestockInsurance([FromBody] RemoveInsuranceLivestockDto insuranceDto)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.ApproveInsuranceRequest(insuranceDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Từ chối đơn bảo hành
        /// </summary>
        [HttpPut("reject-livestock-insurence-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectLivestockInsurance([FromBody] RejectInsuranceDto insuranceDto)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.RejectInsuranceRequest(insuranceDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Bàn giao đơn bảo hành
        /// </summary>
        [HttpPut("transfer-livestock-insurence-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TransferLivestockInsurance([FromBody] RemoveInsuranceLivestockDto insuranceDto)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.TransferInsuranceRequest(insuranceDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
        /// <summary>
        /// Cập nhật lại thông tin đơn bảo hành
        /// </summary>
        [HttpPut("update-livestock-insurence-request-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLivestockInsuranceInfo([FromBody] UpdateInsuranceRequestInfoDto insuranceDto)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.UpdateInfoInsuranceRequest(insuranceDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy các bệnh được bảo hành theo đơn bảo hành
        /// </summary>
        [HttpGet("get-vaccination-by-insurance/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVaccinationByInsurance([FromRoute] String id)
        {
            try
            {

                var codeRangeModel = await _insurenceRepository.GetDataProcurmentByInsurance(id);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy tất cả trạng thái của thu hồi vật nuôi
        /// </summary>
        [HttpGet("get-all-status-retun")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<SpecieTypesDto> GetAllStatusReturn()
        {
            try
            {
                var specieTypes = Enum.GetValues(typeof(insurance_request_livestock_status))
                                       .Cast<insurance_request_livestock_status>()
                                       .Select(e => e.ToString())  // Chuyển giá trị enum thành chuỗi
                                       .ToList();

                SpecieTypesDto result = new SpecieTypesDto
                {
                    Total = specieTypes.Count,
                    Items = specieTypes
                };
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetAllStatusReturn)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
