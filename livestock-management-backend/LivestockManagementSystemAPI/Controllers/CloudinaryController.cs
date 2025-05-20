using DataAccess.Repository.Interfaces;
using DataAccess.Repository.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivestockManagementSystemAPI.Controllers
{
    [ApiController]
    [Route("api/cloudinary")]
    [AllowAnonymous]
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryRepository _cloudinaryRepository;

        public CloudinaryController(ICloudinaryRepository cloudinaryRepository)
        {
            _cloudinaryRepository = cloudinaryRepository;
        }

        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            var result = await _cloudinaryRepository.UploadFileAsync(file);
            return Ok(new { url = result });
        }

        [HttpGet("download-file")]
        public IActionResult DownloadFile(string publicId)
        {
            var fileUrl = _cloudinaryRepository.DownloadFileUrl(publicId);
            return Ok(new { url = fileUrl });
        }
    }
}
