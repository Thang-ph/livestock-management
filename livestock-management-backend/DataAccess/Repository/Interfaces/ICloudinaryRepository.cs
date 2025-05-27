using Microsoft.AspNetCore.Http;

namespace DataAccess.Repository.Interfaces
{
    public interface ICloudinaryRepository
    {
        Task<string> DownloadFileUrl(string publicId);

        Task<string> UploadFileAsync(IFormFile file);

        Task<string> UploadFileStreamAsync(string folderName, string fileName, Stream stream);

        Task<string> UploadFileToFolderAsync(string filePath, string folder, string fileName);
    }
}
