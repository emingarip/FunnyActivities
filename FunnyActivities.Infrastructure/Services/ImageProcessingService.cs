using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Infrastructure.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        public async Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height)
        {
            using var image = Image.Load(imageData);
            image.Mutate(x => x.Resize(width, height));

            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            return ms.ToArray();
        }

        public async Task<bool> ValidateImageAsync(byte[] imageData, string contentType)
        {
            try
            {
                using var image = Image.Load(imageData);
                // Check if it's a valid image format (JPEG or PNG)
                return contentType.ToLower() switch
                {
                    "image/jpeg" => true,
                    "image/jpg" => true,
                    "image/png" => true,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        public async Task<(int Width, int Height)> GetImageDimensionsAsync(byte[] imageData)
        {
            using var image = Image.Load(imageData);
            return (image.Width, image.Height);
        }
    }
}