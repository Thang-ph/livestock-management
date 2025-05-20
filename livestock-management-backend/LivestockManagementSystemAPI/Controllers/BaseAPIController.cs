using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net;
using System.Security.Claims;


namespace LivestockManagementSystemAPI.Controllers
{

    public class BaseResponse : ActionResult
    {
        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public HttpStatusCode StatusCode { get; set; }

        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public IEnumerable<KeyValuePair<string, string>> Errors { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
    }

    public class BaseAPIController : ControllerBase
    {
        public string UserId => User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                            ?? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                            ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        /// <summary>
        /// Errors the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="data">The extend data.</param>
        /// <returns></returns>
        protected ActionResult Error(string message, object data = null)
        {
            return new BadRequestObjectResult(new BaseResponse
            {
                Data = data,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = message
            });
        }

        /// <summary>
        /// Gets the data failed.
        /// </summary>
        /// <returns></returns>
        protected ActionResult GetError()
        {
            return Error("Get Data Failed");
        }

        /// <summary>
        /// Gets the data failed.
        /// </summary>
        /// <returns></returns>
        protected ActionResult GetError(string message)
        {
            return Error(message);
        }

        /// <summary>
        /// Saves the data failed.
        /// </summary>
        /// <returns></returns>
        protected ActionResult SaveError(object data = null)
        {
            return Error("Save Data Failed", data);
        }

        /// <summary>
        /// Models the invalid.
        /// </summary>
        /// <returns></returns>


        /// <summary>
        /// Successes request.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected ActionResult Success(object data, string message)
        {
            return new OkObjectResult(new BaseResponse
            {
                Data = data,
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = message,
                Success = true
            });
        }

        /// <summary>
        /// Gets the data successfully.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected ActionResult GetSuccess(object data)
        {
            return Success(data, "Get Success");
        }

        /// <summary>
        /// Saves the data successfully
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected ActionResult SaveSuccess(object data)
        {
            return Success(data, "Save Success");
        }

    }
}
