using System.Threading.Tasks;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload
{
    public interface IFileProcessor
    {
        Task<byte[]> ProcessFileAsync(byte[] fileData, string contentType, Dictionary<string, string> processingOptions);
        Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height, string format = "jpeg");
        Task<byte[]> CompressImageAsync(byte[] imageData, int quality = 80);
        Task<byte[]> ConvertImageFormatAsync(byte[] imageData, string targetFormat);
        Task<Dictionary<string, string>> ExtractMetadataAsync(byte[] fileData, string contentType);
        Task<bool> IsImageFile(string contentType);
        Task<bool> IsVideoFile(string contentType);
        Task<bool> IsDocumentFile(string contentType);
    }
}