using AutoMapper;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    /// <summary>
    /// API quản lý thông tin trang trại trong hệ thống
    /// </summary>
    [Route("api/barn")]
    [ApiController]
    [SwaggerTag("Quản lý chuồng trại: xem danh sách và thông tin chuồng trại")]
    public class BarnController : BaseAPIController
    {
        private readonly ILogger<BarnController> _logger;
        private readonly IBarnRepository _barnRepository;
        public BarnController(ILogger<BarnController> logger, IBarnRepository barnRepository)
        {
            _barnRepository = barnRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách trang trại
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả các trang trại có trong hệ thống.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "statusCode": 200,
        ///   "success": true,
        ///   "data": {
        ///     "total": 3,
        ///     "items": [
        ///       {
        ///         "id": "B001",
        ///         "name": "Chuồng A1",
        ///         "address": "Huyện Yên Dũng, Tỉnh Bắc Giang"
        ///       },
        ///       {
        ///         "id": "B002",
        ///         "name": "Chuồng A2",
        ///         "address": "Khu A, Trang trại Đồng Nai",
        ///       },
        ///       {
        ///         "id": "B003",
        ///         "name": "Chuồng B1",
        ///         "address": "Khu B, Trang trại Đồng Nai",
        ///       }
        ///     ]
        ///   },
        ///   "errors": null,
        ///   "message": "Get Success"
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách trang trại</returns>
        /// <response code="200">Thành công, trả về danh sách trang trại</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-barns")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListBarns>> GetListBarns()
        {
            try
            {
                var data = await _barnRepository.GetListBarns();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListBarns)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-barn-by-id/{id}")]
        public async Task<ActionResult<BarnInfo>> GetBarnById([FromRoute] string id)
        {
            try
            {
                var data = await _barnRepository.GetBarnById(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListBarns)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPost("create-barn")]
        public async Task<ActionResult<BarnInfo>> CreateBarn([FromBody]CreateBarnDTO createModel)
        {
            try
            {
                var data = await _barnRepository.CreateBarn(createModel);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CreateBarn)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPut("update-barn/{id}")]
        public async Task<ActionResult<BarnInfo>> UpdateBarn([FromRoute] string id, [FromBody] UpdateBarnDTO updateModel)
        {
            try
            {
                var data = await _barnRepository.UpdateBarn(id, updateModel);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateBarn)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpDelete("delete-barn/{id}")]
        public async Task<ActionResult<bool>> DeleteBarn([FromRoute] string id)
        {
            try
            {
                var data = await _barnRepository.DeleteBarn(id);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(DeleteBarn)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
    }
}
