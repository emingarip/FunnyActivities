using FluentAssertions;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Infrastructure.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;
using Xunit;

namespace FunnyActivities.Application.UnitTests
{
    public class ImageProcessingServiceTests
    {
        private readonly IImageProcessingService _imageProcessingService;

        public ImageProcessingServiceTests()
        {
            _imageProcessingService = new ImageProcessingService();
        }

        [Fact]
        public async Task ValidateImageAsync_ShouldReturnTrue_ForValidJpegImage()
        {
            // Arrange
            using var image = new Image<Rgba32>(100, 100);
            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            var imageData = ms.ToArray();

            // Act
            var result = await _imageProcessingService.ValidateImageAsync(imageData, "image/jpeg");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateImageAsync_ShouldReturnTrue_ForValidPngImage()
        {
            // Arrange
            using var image = new Image<Rgba32>(100, 100);
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            var imageData = ms.ToArray();

            // Act
            var result = await _imageProcessingService.ValidateImageAsync(imageData, "image/png");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateImageAsync_ShouldReturnFalse_ForInvalidContentType()
        {
            // Arrange
            using var image = new Image<Rgba32>(100, 100);
            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            var imageData = ms.ToArray();

            // Act
            var result = await _imageProcessingService.ValidateImageAsync(imageData, "image/gif");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateImageAsync_ShouldReturnFalse_ForInvalidImageData()
        {
            // Arrange
            var invalidImageData = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = await _imageProcessingService.ValidateImageAsync(invalidImageData, "image/jpeg");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ResizeImageAsync_ShouldResizeImageCorrectly()
        {
            // Arrange
            using var originalImage = new Image<Rgba32>(200, 200);
            using var ms = new MemoryStream();
            await originalImage.SaveAsJpegAsync(ms);
            var imageData = ms.ToArray();

            // Act
            var resizedData = await _imageProcessingService.ResizeImageAsync(imageData, 150, 150);

            // Assert
            using var resizedImage = Image.Load(resizedData);
            resizedImage.Width.Should().Be(150);
            resizedImage.Height.Should().Be(150);
        }

        [Fact]
        public async Task GetImageDimensionsAsync_ShouldReturnCorrectDimensions()
        {
            // Arrange
            using var image = new Image<Rgba32>(300, 400);
            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            var imageData = ms.ToArray();

            // Act
            var dimensions = await _imageProcessingService.GetImageDimensionsAsync(imageData);

            // Assert
            dimensions.Width.Should().Be(300);
            dimensions.Height.Should().Be(400);
        }
    }
}