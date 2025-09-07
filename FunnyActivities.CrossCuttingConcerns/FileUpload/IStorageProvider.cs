using System.Threading.Tasks;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload
{
    public interface IStorageProvider
    {
        Task<string> UploadFileAsync(string bucketName, string objectKey, byte[] fileData, string contentType, Dictionary<string, string> metadata = null);
        Task<byte[]> DownloadFileAsync(string bucketName, string objectKey);
        Task<bool> DeleteFileAsync(string bucketName, string objectKey);
        Task<string> GeneratePreSignedUrlAsync(string bucketName, string objectKey, int expiryInSeconds = 3600);
        Task<bool> FileExistsAsync(string bucketName, string objectKey);
        Task<FileMetadata> GetFileMetadataAsync(string bucketName, string objectKey);
        Task<List<string>> ListFilesAsync(string bucketName, string prefix = null);
        Task<bool> CreateBucketAsync(string bucketName);
        Task<bool> BucketExistsAsync(string bucketName);
        Task<bool> DeleteBucketAsync(string bucketName);
    }
}