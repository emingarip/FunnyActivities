using System;
using FluentAssertions;
using FunnyActivities.Domain.ValueObjects;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class VideoUrlTests
    {
        [Fact]
        public void Create_ShouldCreateVideoUrlWithValidUrl()
        {
            // Arrange
            var validUrl = "https://example.com/video.mp4";

            // Act
            var videoUrl = VideoUrl.Create(validUrl);

            // Assert
            videoUrl.Value.Should().Be(validUrl);
        }

        [Theory]
        [InlineData("http://example.com/video.mp4")]
        [InlineData("https://example.com/video.mp4")]
        public void Create_ShouldAcceptHttpAndHttpsUrls(string url)
        {
            // Act
            var videoUrl = VideoUrl.Create(url);

            // Assert
            videoUrl.Value.Should().Be(url);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldThrowArgumentException_WhenUrlIsNullOrEmpty(string invalidUrl)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => VideoUrl.Create(invalidUrl));
        }

        [Theory]
        [InlineData("ftp://example.com/video.mp4")]
        [InlineData("invalid-url")]
        [InlineData("example.com/video.mp4")]
        [InlineData("mailto:test@example.com")]
        [InlineData("tel:+1234567890")]
        public void Create_ShouldThrowArgumentException_WhenUrlIsInvalid(string invalidUrl)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => VideoUrl.Create(invalidUrl));
        }

        [Theory]
        [InlineData("data:video/mp4;base64,AAAAHGZ0eXBtcDQyAAACAGlzb21pc28yYXZjMQAAAAhmcmVlAAAGF21kYXQQAAACBAAlEAAgQJ")]
        [InlineData("blob:https://example.com/12345678-1234-1234-1234-123456789012")]
        [InlineData("file:///path/to/video.mp4")]
        [InlineData("rtmp://example.com/live/stream")]
        [InlineData("rtsp://example.com:554/stream")]
        [InlineData("mms://example.com/stream")]
        public void Create_ShouldAcceptValidUriSchemes(string validUrl)
        {
            // Act
            var videoUrl = VideoUrl.Create(validUrl);

            // Assert
            videoUrl.Value.Should().Be(validUrl);
        }

        [Theory]
        [InlineData("minio-object-key")]
        [InlineData("video-file-name.mp4")]
        [InlineData("path/to/video.mp4")]
        [InlineData("12345678-1234-1234-1234-123456789012")] // UUID-like object key
        public void Create_ShouldAcceptMinioObjectKeys(string objectKey)
        {
            // Act
            var videoUrl = VideoUrl.Create(objectKey);

            // Assert
            videoUrl.Value.Should().Be(objectKey);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        [InlineData("video\x00url")] // Contains null character
        public void Create_ShouldThrowArgumentException_WhenUrlContainsInvalidCharacters(string invalidUrl)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => VideoUrl.Create(invalidUrl));
        }

        [Fact]
        public void ToString_ShouldReturnValue()
        {
            // Arrange
            var url = "https://example.com/video.mp4";
            var videoUrl = VideoUrl.Create(url);

            // Act
            var result = videoUrl.ToString();

            // Assert
            result.Should().Be(url);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenValuesAreEqual()
        {
            // Arrange
            var url1 = "https://example.com/video.mp4";
            var url2 = "https://example.com/video.mp4";
            var videoUrl1 = VideoUrl.Create(url1);
            var videoUrl2 = VideoUrl.Create(url2);

            // Act & Assert
            videoUrl1.Equals(videoUrl2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenValuesAreDifferent()
        {
            // Arrange
            var videoUrl1 = VideoUrl.Create("https://example.com/video1.mp4");
            var videoUrl2 = VideoUrl.Create("https://example.com/video2.mp4");

            // Act & Assert
            videoUrl1.Equals(videoUrl2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ShouldReturnSameHashCode_WhenValuesAreEqual()
        {
            // Arrange
            var url = "https://example.com/video.mp4";
            var videoUrl1 = VideoUrl.Create(url);
            var videoUrl2 = VideoUrl.Create(url);

            // Act & Assert
            videoUrl1.GetHashCode().Should().Be(videoUrl2.GetHashCode());
        }
    }
}