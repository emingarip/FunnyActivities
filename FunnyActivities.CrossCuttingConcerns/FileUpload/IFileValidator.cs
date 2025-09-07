using System.Threading.Tasks;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload
{
    public interface IFileValidator
    {
        Task<FileValidationResult> ValidateFileAsync(FileUploadRequest request);
        Task<FileValidationResult> ValidateFilesAsync(IEnumerable<FileUploadRequest> requests);
        bool IsFileTypeAllowed(string contentType, string fileType);
        bool IsFileSizeValid(long fileSize, string fileType);
    }
}