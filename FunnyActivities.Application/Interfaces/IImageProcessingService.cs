using System.Threading.Tasks;

namespace FunnyActivities.Application.Interfaces
{
    public interface IImageProcessingService
    {
        Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height);
        Task<bool> ValidateImageAsync(byte[] imageData, string contentType);
        Task<(int Width, int Height)> GetImageDimensionsAsync(byte[] imageData);
    }
}