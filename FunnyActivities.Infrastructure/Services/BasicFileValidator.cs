using System.Threading.Tasks;
using FunnyActivities.CrossCuttingConcerns.FileUpload;
using FunnyActivities.CrossCuttingConcerns.FileUpload.Configuration;

namespace FunnyActivities.Infrastructure.Services
{
    public class BasicFileValidator : IFileValidator
    {
        private readonly FileUploadConfiguration _configuration;

        public BasicFileValidator(FileUploadConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<FileValidationResult> ValidateFileAsync(FileUploadRequest request)
        {
            var result = new FileValidationResult();

            // Check file size
            if (!IsFileSizeValid(request.FileData.Length, request.FileType))
            {
                result.Errors.Add($"File size {request.FileData.Length} exceeds maximum allowed size");
                result.IsValid = false;
            }

            // Check content type
            if (!IsFileTypeAllowed(request.ContentType, request.FileType))
            {
                result.Errors.Add($"Content type {request.ContentType} is not allowed for file type {request.FileType}");
                result.IsValid = false;
            }

            return await Task.FromResult(result);
        }

        public async Task<FileValidationResult> ValidateFilesAsync(IEnumerable<FileUploadRequest> requests)
        {
            var result = new FileValidationResult();

            foreach (var request in requests)
            {
                var singleResult = await ValidateFileAsync(request);
                if (!singleResult.IsValid)
                {
                    result.Errors.AddRange(singleResult.Errors);
                    result.IsValid = false;
                }
            }

            return result;
        }

        public bool IsFileTypeAllowed(string contentType, string fileType)
        {
            if (_configuration.FileTypeConfigurations.TryGetValue(fileType, out var fileTypeConfig))
            {
                return fileTypeConfig.AllowedContentTypes.Contains(contentType);
            }

            return _configuration.AllowedFileTypes.Contains(contentType);
        }

        public bool IsFileSizeValid(long fileSize, string fileType)
        {
            if (_configuration.FileTypeConfigurations.TryGetValue(fileType, out var fileTypeConfig))
            {
                return fileSize >= fileTypeConfig.MinFileSize && fileSize <= fileTypeConfig.MaxFileSize;
            }

            return fileSize <= _configuration.MaxFileSize;
        }
    }
}