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
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;
        public OrderController(IOrderRepository orderRepository, ILogger<OrderController> logger)
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

        [HttpPost("complete-order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CompleteOrder([FromRoute] string orderId, string requestedBy)
        {
            try
            {
                var data = await _orderRepository.CompletelOrder(orderId, requestedBy);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CompleteOrder)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }


        [HttpPost("cancel-order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CancelOrder([FromRoute] string orderId, string requestedBy)
        {
            try
            {
                var data = await _orderRepository.CancelOrder(orderId, requestedBy);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CancelOrder)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }


        [HttpPost("confirm-exported/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ConfirmExportedLivestock([FromRoute] string orderId, string requestedBy)
        {
            try
            {
                var data = await _orderRepository.ConfirmExport(orderId, requestedBy);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ConfirmExportedLivestock)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPost("remove-request-choose/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> RemoveRequestChooseOrder([FromRoute] string orderId)
        {
            try
            {
                var data = await _orderRepository.RemoveRequestChoose(orderId);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(RemoveRequestChooseOrder)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPost("request-choose/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> RequestChooseOrder([FromRoute] string orderId)
        {
            try
            {
                var data = await _orderRepository.RequestChoose(orderId);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(RequestChooseOrder)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPost("request-export/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> RequestExportOrder([FromRoute] string orderId)
        {
            try
            {
                var data = await _orderRepository.RequestExport(orderId);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(RequestExportOrder)} " + ex.Message);
                return SaveError(ex.Message);
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

        [HttpGet("get-list-orders-to-choose")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListOrders>> GetListOrderToChoose()
        {
            try
            {
                var data = await _orderRepository.GetListOrderToChoose();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListOrderToChoose)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-list-orders-to-export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListOrderExport>> GetListOrderToExport()
        {
            try
            {
                var data = await _orderRepository.GetListOrderToExport();
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListOrderToExport)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPost("create-order")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDetailsDTO>> CreateOrder([FromBody] CreateOrderDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"[{this.GetType().Name}]/{nameof(CreateOrder)} ModelState not Valid");
                    throw new Exception(string.Join("; ", ModelState.Values
                                              .SelectMany(x => x.Errors)
                                              .Select(x => x.ErrorMessage)));
                }
                //request.RequestedBy = UserId;
                var data = await _orderRepository.CreateOrder(request);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(CreateOrder)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPut("update-order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDetailsDTO>> UpdateOrder([FromRoute] string orderId, [FromBody] UpdateOrderDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(UpdateOrder)} ModelState not Valid");
                    throw new Exception(string.Join("; ", ModelState.Values
                                              .SelectMany(x => x.Errors)
                                              .Select(x => x.ErrorMessage)));
                }
                var data = await _orderRepository.UpdateOrder(orderId, request);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(UpdateOrder)} " + ex.Message);
                return SaveError(ex.Message);

            }
        }

        [HttpGet("get-order-details-info/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDetailsDTO>> GetOrderDetailsById([FromRoute] string orderId)
        {
            try
            {
                var data = await _orderRepository.GetOrderDetailsById(orderId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetOrderDetailsById)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("get-list-order-livestock-details/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListOrderLivestocks>> GetListLivestockInOrderDetails([FromRoute] string orderId, [FromQuery] ListLivestockFilter filter )
        {
            try
            {
                var data = await _orderRepository.GetListLivestockInOrder(orderId, filter);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetListLivestockInOrderDetails)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpDelete("delete-livestock-from-order/{orderDetailsId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeleteLivestockFromOrder([FromRoute] string orderDetailsId)
        {
            try
            {
                var result = await _orderRepository.DeleteLivestockFromOrder(orderDetailsId);
                return SaveSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(DeleteLivestockFromOrder)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpPost("add-livestock-to-order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LivestockBatchImportInfo>> AddLivestockToOrder([FromRoute] string orderId, [FromBody] AddLivestockToOrderDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"[{this.GetType().Name}]/{nameof(AddLivestockToOrder)} ModelState not Valid");
                    throw new Exception(string.Join("; ", ModelState.Values
                                              .SelectMany(x => x.Errors)
                                              .Select(x => x.ErrorMessage)));
                }
                //request.RequestedBy = UserId;
                var data = await _orderRepository.AddLivestockToOrder(orderId, model);
                return SaveSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddLivestockToOrder)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }

        [HttpGet("template-to-choose-livestock/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetTemplateToChooseLivestock([FromRoute] string orderId)
        {
            try
            {
                var fileUrl = await _orderRepository.GetTemplateToChooseLivestock(orderId);
                return GetSuccess(fileUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetTemplateToChooseLivestock)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpGet("template-order-report/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetOrderReportFile([FromRoute] string orderId)
        {
            try
            {
                var fileUrl = await _orderRepository.GetReportedFile(orderId);
                return GetSuccess(fileUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetOrderReportFile)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPost("import-list-chosed-livestock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ImportListChosedLivestock([FromForm] string orderId, [FromForm] string requestedBy, IFormFile file)
        {
            try
            {
                await _orderRepository.ImportListChosedLivestock(orderId, requestedBy, file);
                return SaveSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(ImportListChosedLivestock)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
    }
}
