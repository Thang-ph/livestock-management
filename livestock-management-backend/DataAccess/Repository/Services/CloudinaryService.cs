using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DataAccess.Repository.Services
{
    public class CloudinaryService : ICloudinaryRepository
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file.Length <= 0) return null;

            await using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = Path.GetFileNameWithoutExtension(file.FileName),
                //ResourceType = "raw"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString();
        }

        public async Task<string> DownloadFileUrl(string publicId)
        {
            var url = _cloudinary.Api.UrlImgUp
                .ResourceType("raw")
                .BuildUrl($"{publicId}");

            return url;
        }

        public async Task<string> UploadFileToFolderAsync(string filePath, string folder, string fileName)
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(filePath),
                Folder = folder,
                PublicId = fileName,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }

        public async Task<string> UploadFileStreamAsync(string folderName, string fileName, Stream stream)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("File name is missing");

            // Create a real temp file with the correct extension
            var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

            using (var fileStream = File.Create(tempFilePath))
            {
                await stream.CopyToAsync(fileStream);
            }

            try
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(tempFilePath),
                    Folder = folderName,
                    //PublicId = Path.Combine(folderName, Path.GetFileNameWithoutExtension(fileName)).Replace("\\", "/")
                    PublicId = fileName,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult?.SecureUrl?.ToString();
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }
    }
}
