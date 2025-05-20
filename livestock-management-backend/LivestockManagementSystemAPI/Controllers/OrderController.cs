using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/order-management")]
    [ApiController]
    //[Authorize] // Changed from AllowAnonymous to Authorize for security
    [AllowAnonymous]
    [SwaggerTag("Quản lý nhập gia súc: tạo, cập nhật, và theo dõi các lô nhập gia súc")]
    public class OrderController : BaseAPIController
    {
        private readonly IOderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;
        public OrderController(IOderRepository orderRepository, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpGet("get-list-order-statuses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetListOrderStatuses()
        {
            try
            {
                var data = Enum.GetNames(typeof(order_status)).ToList();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListOrderStatuses)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-list-orders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListOrders>> GetListOrder([FromQuery] ListOrderFilter filter)
        {
            try
            {
                var data = await _orderRepository.GetListOrder(filter);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListOrder)} " + ex.Message);
                return GetError(ex.Message);
            }
        }
    }
}
