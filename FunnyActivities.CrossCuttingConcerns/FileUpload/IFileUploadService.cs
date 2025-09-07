using System.Threading.Tasks;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload
{
    public interface IFileUploadService
    {
        Task<FileUploadResult> UploadFileAsync(FileUploadRequest request);
        Task<FileUploadResult> UploadFilesAsync(IEnumerable<FileUploadRequest> requests);
        Task<bool> DeleteFileAsync(string fileId);
        Task<string> GetFileUrlAsync(string fileId, int expiryInSeconds = 3600);
        Task<FileMetadata> GetFileMetadataAsync(string fileId);
    }
}