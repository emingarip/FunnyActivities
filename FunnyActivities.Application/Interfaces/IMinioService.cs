using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Enums;
using FunnyActivities.Domain.ValueObjects;

namespace FunnyActivities.Application.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadImageAsync(byte[] imageData, string fileName, string contentType, string imageType);
        Task<string> GeneratePreSignedUrlAsync(string objectKey, int expiryInSeconds = 3600);
        Task<bool> DeleteImageAsync(string objectKey);
        Task<Image> SaveImageMetadataAsync(Image image);

        // Video-specific methods
        Task<string> UploadVideoAsync(byte[] videoData, string fileName, string contentType, Guid activityId, ActivityVideoType videoType = ActivityVideoType.Main);
        Task<string> GenerateVideoPreSignedUrlAsync(string objectKey, int expiryInSeconds = 3600);
        Task<bool> DeleteVideoAsync(string objectKey);

        // HEAD request methods for metadata checking (legacy - consider using GET-based methods below)
        Task<string> GeneratePreSignedHeadUrlAsync(string objectKey, int expiryInSeconds = 3600);
        Task<string> GenerateVideoPreSignedHeadUrlAsync(string objectKey, int expiryInSeconds = 3600);

        // GET-based metadata retrieval methods (new - replaces HEAD requests)
        Task<ObjectMetadata> GetObjectMetadataAsync(string objectKey);
        Task<ObjectMetadata> GetVideoMetadataAsync(string objectKey);
    }
}
