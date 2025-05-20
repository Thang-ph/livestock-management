using System.Threading.Tasks;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/inspection-code-range")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Quản lý mã kiểm dịch: thêm mới, tìm kiếm, danh sách các đợt mã kiểm dịch đã tạo")]
    public class InspectionCodeRangeController : BaseAPIController
    {
        private readonly IInspectionCodeRangeRepository _codeRangeRepository;
        private readonly ILogger<LivestockController> _logger;
        private readonly IInspectionCodeCounterRepository _codeCounterRepository;

        public InspectionCodeRangeController(IInspectionCodeRangeRepository codeRangeRepository, IInspectionCodeCounterRepository codeCounterRepository, ILogger<LivestockController> logger)
        {
            _codeRangeRepository = codeRangeRepository;
            _logger = logger;
            _codeCounterRepository = codeCounterRepository;
        }

        /// <summary>
        /// Tạo mới bảng mã
        /// </summary>
        [HttpPost("create-inspection-code-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateInspectionCodeRangeDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Create)} ModelState not Valid");
                    return GetError("ModelState not Valid");
                }
                var codeRangeModel = await _codeRangeRepository.CreateInspectionCodeRange(createDto);
                return SaveSuccess(codeRangeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Truy vấn bảng theo lọc, api dùng cho màn list và màn detail của vật nuôi phải dùng lọc
        /// </summary>
        [HttpGet("get-all-inspection-code-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListInspectionCodeRanges>> GetList([FromQuery] InspectionCodeRangeFilter codeRangeFilter)
        {
            try
            {
                var data = await _codeRangeRepository.GetListInspectionCodeRange(codeRangeFilter);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetList)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy chi tiết của mã nào đó theo id
        /// </summary>
        [HttpGet("get-inspection-code-range-info/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InspectionCodeRangeDTO>> GetInspectionCodeRangeInfo([FromRoute] string id)
        {
            try
            {
                var data = await _codeRangeRepository.GetInspcetionCodeRangeById(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetInspectionCodeRangeInfo)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// lấy tất cả các loại vật nuôi ra như TRÂU, BÒ
        /// </summary>
        [HttpGet("get-all-specie")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<SpecieTypesDto> GetAllSpecie()
        {
            try
            {
                var specieTypes = Enum.GetValues(typeof(specie_type))
                                       .Cast<specie_type>()
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
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetAllSpecie)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy trạng thái của codeRange
        /// </summary>
        [HttpGet("get-all-status-code-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<SpecieTypesDto> GetAllStatusCodeRange()
        {
            try
            {
                var specieTypes = Enum.GetValues(typeof(inspection_code_range_status))
                                       .Cast<inspection_code_range_status>()
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
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetAllStatusCodeRange)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật lại một code range nào đó chỉ cập nhật được khi trạng thái là MỚI
        /// </summary>
        [HttpPut("update-code-range/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InspectionCodeRangeDTO>> UpdateCodeRange([FromRoute] string id,[FromBody] CreateInspectionCodeRangeDTO request)
        {
            try
            {
             var result = await _codeRangeRepository.UpdateInspectionCodeRange(id, request);
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateCodeRange)} " + ex.Message);

                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy tất cả theo loại vật nuôi cho màn vật nuôi
        /// </summary>
        [HttpGet("get-all-code-ranges-by-types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LstInspectionCodeRangesDto>> GetAllCodeRangeByType()
        {
            try
            {
                var result = await _codeRangeRepository.GetListInspectionByType();
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetAllCodeRangeByType)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
          


        /// <summary>
        /// Lấy cảnh báo cho vật nuôi
        /// </summary>
        [HttpGet("get-warrning")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LstInspectionCodeRangesDto>> GetWarrning()
        {
            try
            {
                var result = await _codeRangeRepository.GetListWarrningInspection();
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetWarrning)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của loài nào đó
        /// </summary>
        [HttpGet("get-info-detail-by-specie/{specie}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InfoSpecieInspectionCodeRangeDto>> GetInfoDetailBySpecie([FromRoute] string specie)
        {
            try
            {
                var result = await _codeRangeRepository.GetInfoSpecieInspectionCodeRange(specie);
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetInfoDetailBySpecie)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của loài nào đó
        /// </summary>
        [HttpGet("get-list-code-ranges-by-specie/{specie}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListInspectionCodeRanges>> GetListInspectionCodeRangesBySpecie([FromRoute] string specie)
        {
            try
            {
                var result = await _codeRangeRepository.GetListInspectionCodeRangeBySpecie(specie);
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListInspectionCodeRangesBySpecie)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("revice-an-inspection-code/{specie}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListInspectionCodeRanges>> GetAnInspectionCode([FromRoute] string specie)
        {
            try
            {
                var result = await _codeCounterRepository.UpdateCurrentNumberInspectionCode(specie);
                return GetSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetAnInspectionCode)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
