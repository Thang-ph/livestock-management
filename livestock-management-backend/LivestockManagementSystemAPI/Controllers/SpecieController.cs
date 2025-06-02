using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/specie-mangament")]
    [ApiController]
    [SwaggerTag("Quản lý loài vật: tạo, cập nhật, và tra cứu thông tin các loài vật trong hệ thống")]
    public class SpecieController : BaseAPIController
    {
        private readonly ISpecieRepository _specieService;
        private readonly ILogger<SpecieController> _logger;


        public SpecieController(ISpecieRepository specieService, ILogger<SpecieController> logger)
        {
            _specieService = specieService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của loài vật theo ID
        /// </summary>
        /// <remarks>
        /// API này trả về thông tin chi tiết của một loài vật theo ID cung cấp.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "SP001",
        ///     "name": "Bò sữa",               -> |Tên loài vật|
        ///     "description": "Bò sữa Holstein",  -> |Mô tả về loài vật|
        ///     "growthRate": 1.2,              -> |Tỷ lệ tăng trưởng|
        ///     "dressingPercentage": 0.7,      -> |Tỷ lệ thịt hữu dụng|
        ///     "type": "BÒ",                   -> |Loại của loài vật (TRÂU, BÒ, LỢN...)|
        ///     "createdBy": "admin",           -> |Người tạo|
        ///     "createdAt": "2023-01-15T10:30:00", -> |Thời gian tạo|
        ///     "updatedBy": "admin",           -> |Người cập nhật gần nhất|
        ///     "updatedAt": "2023-01-15T10:30:00"  -> |Thời gian cập nhật gần nhất|
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của loài vật cần lấy thông tin</param>
        /// <returns>Thông tin chi tiết của loài vật</returns>
        /// <response code="200">Thành công, trả về thông tin loài vật</response>
        /// <response code="404">Không tìm thấy loài vật với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SpecieDTO>> GetById([FromRoute] string id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(GetById)} ModelState not Valid");
                    return GetError("ModelState not Valid");
                }
                var specie = await _specieService.GetByIdAsync(id);
                return GetSuccess(specie);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetById)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả các loài vật
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả các loài vật trong hệ thống.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "id": "SP001",
        ///       "name": "Bò sữa",               -> |Tên loài vật|
        ///       "description": "Bò sữa Holstein",  -> |Mô tả về loài vật|
        ///       "growthRate": 1.2,              -> |Tỷ lệ tăng trưởng|
        ///       "dressingPercentage": 0.7,      -> |Tỷ lệ thịt hữu dụng|
        ///       "type": "BÒ",                   -> |Loại của loài vật|
        ///       "createdBy": "admin",           -> |Người tạo|
        ///       "createdAt": "2023-01-15T10:30:00", -> |Thời gian tạo|
        ///       "updatedBy": "admin",           -> |Người cập nhật gần nhất|
        ///       "updatedAt": "2023-01-15T10:30:00"  -> |Thời gian cập nhật gần nhất|
        ///     },
        ///     {
        ///       "id": "SP002",
        ///       "name": "Lợn rừng",
        ///       "description": "Lợn rừng lai",
        ///       "growthRate": 0.8,
        ///       "dressingPercentage": 0.65,
        ///       "type": "LỢN",
        ///       "createdBy": "admin",
        ///       "createdAt": "2023-01-20T14:15:00",
        ///       "updatedBy": "admin",
        ///       "updatedAt": "2023-01-20T14:15:00"
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách tất cả các loài vật</returns>
        /// <response code="200">Thành công, trả về danh sách loài vật</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<SpecieDTO>>> GetAll()
        {
            try
            {
                var species = await _specieService.GetAllAsync();
                return GetSuccess(species);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetAll)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Tạo mới một loài vật
        /// </summary>
        /// <remarks>
        /// API này thực hiện tạo mới một loài vật trong hệ thống.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Bò sữa Holstein",       -> |Tên loài vật|
        ///   "description": "Bò sữa nhập khẩu từ Úc", -> |Mô tả về loài vật|
        ///   "growthRate": 1.2,               -> |Tỷ lệ tăng trưởng|
        ///   "dressingPercentage": 0.7,       -> |Tỷ lệ thịt hữu dụng|
        ///   "type": "BÒ",                    -> |Loại của loài vật (TRÂU, BÒ, LỢN...)|
        ///   "createdBy": "admin"             -> |Người tạo|
        /// }
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "SP003",
        ///     "name": "Bò sữa Holstein",
        ///     "description": "Bò sữa nhập khẩu từ Úc",
        ///     "growthRate": 1.2,
        ///     "dressingPercentage": 0.7,
        ///     "type": "BÒ",
        ///     "createdBy": "admin",
        ///     "createdAt": "2023-03-10T08:45:00",
        ///     "updatedBy": "admin",
        ///     "updatedAt": "2023-03-10T08:45:00"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="specieCreate">Thông tin loài vật cần tạo</param>
        /// <returns>Thông tin loài vật sau khi tạo</returns>
        /// <response code="200">Thành công, trả về thông tin loài vật đã tạo</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SpecieDTO>> Create([FromBody] SpecieCreate specieCreate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Create)} ModelState not Valid");
                    return GetError("ModelState not Valid");
                }
                var createdSpecie = await _specieService.CreateAsync(specieCreate);
                return SaveSuccess(createdSpecie);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Create)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin loài vật
        /// </summary>
        /// <remarks>
        /// API này thực hiện cập nhật thông tin của một loài vật đã tồn tại.
        /// 
        /// Ví dụ Request Body:
        /// ```json
        /// {
        ///   "name": "Bò sữa Holstein cập nhật",   -> |Tên loài vật mới|
        ///   "description": "Bò sữa cao sản",      -> |Mô tả mới về loài vật|
        ///   "growthRate": 1.3,                    -> |Tỷ lệ tăng trưởng mới|
        ///   "dressingPercentage": 0.75,           -> |Tỷ lệ thịt hữu dụng mới|
        ///   "type": "BÒ",                         -> |Loại của loài vật|
        ///   "updatedBy": "admin"                  -> |Người cập nhật|
        /// }
        /// ```
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "SP001",
        ///     "name": "Bò sữa Holstein cập nhật",
        ///     "description": "Bò sữa cao sản",
        ///     "growthRate": 1.3,
        ///     "dressingPercentage": 0.75,
        ///     "type": "BÒ",
        ///     "createdBy": "admin",
        ///     "createdAt": "2023-01-15T10:30:00",
        ///     "updatedBy": "admin",
        ///     "updatedAt": "2023-04-05T16:20:00"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của loài vật cần cập nhật</param>
        /// <param name="specieUpdate">Thông tin cập nhật cho loài vật</param>
        /// <returns>Thông tin loài vật sau khi cập nhật</returns>
        /// <response code="200">Thành công, trả về thông tin loài vật đã cập nhật</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy loài vật với ID cung cấp</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SpecieDTO>> Update([FromRoute] string id, [FromBody] SpecieUpdate specieUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(Update)} ModelState not Valid");
                    return GetError("ModelState not Valid");
                }
                var updatedSpecie = await _specieService.UpdateAsync(id, specieUpdate);
                return SaveSuccess(updatedSpecie);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Update)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Xóa loài vật
        /// </summary>
        /// <remarks>
        /// API này thực hiện xóa một loài vật khỏi hệ thống.
        /// Chỉ những loài vật không được sử dụng trong các dữ liệu liên quan (gia súc, gói thầu) mới có thể xóa.
        /// 
        /// Ví dụ Response khi xóa thành công:
        /// ```json
        /// {
        ///   "data": {
        ///     "id": "SP002",
        ///     "name": "Lợn rừng",
        ///     "description": "Lợn rừng lai",
        ///     "growthRate": 0.8,
        ///     "dressingPercentage": 0.65,
        ///     "type": "LỢN",
        ///     "createdBy": "admin",
        ///     "createdAt": "2023-01-20T14:15:00",
        ///     "updatedBy": "admin",
        ///     "updatedAt": "2023-01-20T14:15:00"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID của loài vật cần xóa</param>
        /// <returns>Thông tin của loài vật đã xóa</returns>
        /// <response code="200">Thành công, đã xóa loài vật</response>
        /// <response code="404">Không tìm thấy loài vật với ID cung cấp</response>
        /// <response code="400">Không thể xóa loài vật đang được sử dụng</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Delete([FromRoute] string id)
        {
            try
            {
                var deletedSpecie = await _specieService.DeleteAsync(id);
                return SaveSuccess(deletedSpecie);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(Delete)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách loài vật có thể xóa
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các loài vật có thể xóa khỏi hệ thống.
        /// Chỉ những loài vật không được sử dụng trong các dữ liệu liên quan (gia súc, gói thầu) mới có thể xóa.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "name": "Lợn rừng núi",          -> |Tên loài vật|
        ///       "description": "Lợn rừng núi cao", -> |Mô tả về loài vật|
        ///       "growthRate": 0.9,               -> |Tỷ lệ tăng trưởng|
        ///       "dressingPercentage": 0.68,      -> |Tỷ lệ thịt hữu dụng|
        ///       "type": "LỢN",                   -> |Loại của loài vật|
        ///       "createdBy": "admin",            -> |Người tạo|
        ///       "updatedBy": "admin"             -> |Người cập nhật gần nhất|
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách các loài vật có thể xóa</returns>
        /// <response code="200">Thành công, trả về danh sách loài vật có thể xóa</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-can-delete-species")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<SpecieDTO>>> GetListCanDeleteSpecies()
        {
            try
            {

                List<SpecieDTO> canDeleteSpecie = await _specieService.GetListCanDeleteSpecies();
                return GetSuccess(canDeleteSpecie);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListCanDeleteSpecies)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách loại của loài vật
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách các loại của loài vật đã được định nghĩa trong hệ thống.
        /// Các loại bao gồm: TRÂU, BÒ, LỢN, GÀ, DÊ, CỪU, NGỰA, LA, LỪA.
        /// 
        /// Ví dụ Response:
        /// ```json
        /// {
        ///   "data": [
        ///     {
        ///       "typeNumber": 0,              -> |Mã số của loại|
        ///       "typeString": "TRÂU"          -> |Tên loại|
        ///     },
        ///     {
        ///       "typeNumber": 1,
        ///       "typeString": "BÒ"
        ///     },
        ///     {
        ///       "typeNumber": 2,
        ///       "typeString": "LỢN"
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Danh sách các loại của loài vật</returns>
        /// <response code="200">Thành công, trả về danh sách loại loài vật</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
        [HttpGet("get-list-specie-type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetListSpecieType()
        {
            try
            {
                var data = Enum.GetNames(typeof(specie_type)).ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListSpecieType)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-specie-by-id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SpecieDTO?>> GetSpecieById([FromRoute] string id)
        {
            try
            {
                var data = await _specieService.GetByIdAsync(id);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetSpecieById)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-list-specie-name/{type}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<SpecieName>>> GetListSpecieNameByType([FromRoute] specie_type type)
        {
            try
            {
                var data = await _specieService.GetListSpecieNameByType(type);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListSpecieNameByType)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
