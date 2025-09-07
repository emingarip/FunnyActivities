using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadImageAsync(byte[] imageData, string fileName, string contentType, string imageType);
        Task<string> GeneratePreSignedUrlAsync(string objectKey, int expiryInSeconds = 3600);
        Task<bool> DeleteImageAsync(string objectKey);
        Task<Image> SaveImageMetadataAsync(Image image);

        // Video-specific methods
        Task<string> UploadVideoAsync(byte[] videoData, string fileName, string contentType, Guid activityId);
        Task<string> GenerateVideoPreSignedUrlAsync(string objectKey, int expiryInSeconds = 3600);
        Task<bool> DeleteVideoAsync(string objectKey);
    }
}