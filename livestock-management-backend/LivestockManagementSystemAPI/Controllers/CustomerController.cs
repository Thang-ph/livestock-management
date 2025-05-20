using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystemAPI.Controllers
{
    [Route("api/customer-management")]
    [ApiController]
    //[Authorize] // Changed from AllowAnonymous to Authorize for security
    [AllowAnonymous]
    [SwaggerTag("Quản lý nhập gia súc: tạo, cập nhật, và theo dõi các lô nhập gia súc")]
    public class CustomerController : BaseAPIController
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerController> _logger;
        public CustomerController(ICustomerRepository customerRepository, ILogger<CustomerController> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        [HttpGet("get-customer-info/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerInfo>> GetCustomerInfomation([FromRoute] string customerId)
        {
            try
            {
                var data = await _customerRepository.GetCustomerInfo(customerId);
                return GetSuccess(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(GetCustomerInfomation)} " + ex.Message);
                return GetError(ex.Message);
            }
        }

        [HttpPost("add-customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerInfo>> AddCustomer([FromBody] AddCustomerDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"[{this.GetType().Name}]/{nameof(AddCustomer)} : ModelState Errors: {errors}");
                    return GetError("ModelState not Valid");
                }
                var batchImport = await _customerRepository.AddCustomer(model);
                return SaveSuccess(batchImport);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{this.GetType().Name}]/{nameof(AddCustomer)} " + ex.Message);
                return SaveError(ex.Message);
            }
        }
    }
}
