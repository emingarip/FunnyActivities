using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Moq;
using Xunit;
using FunnyActivities.Infrastructure.Services;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;

namespace HeadRequestFallbackTest
{
    public class HeadRequestFallbackTest
    {
        [Fact]
        public async Task GeneratePreSignedHeadUrlAsync_With403Error_ShouldFallbackToGetBasedMethod()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MinioService>>();
            var minioClientMock = new Mock<IMinioClient>();
            var contextMock = new Mock<ApplicationDbContext>();

            var minioConfig = new MinioConfiguration
            {
                Endpoint = "minio:9000",
                ExternalEndpoint = "localhost:9000",
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                UseSSL = false,
                Region = "us-east-1"
            };

            var sut = new MinioService(minioClientMock.Object, contextMock.Object, minioConfig, loggerMock.Object);
            var objectKey = "videos/activity-123/test-video.mp4";
            var expectedGetUrl = "http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

            // Setup mocks
            minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
                .ReturnsAsync(true);

            minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
                .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

            // Create a separate mock for the external client that throws 403 error
            var externalMinioClientMock = new Mock<IMinioClient>();
            externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
                .ThrowsAsync(new Exception("403 Forbidden: Access denied"));

            // Setup the MinioClient factory to return our external client mock
            minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains("localhost:9000"))))
                .Returns(externalMinioClientMock.Object);
            minioClientMock.Setup(x => x.WithCredentials(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(externalMinioClientMock.Object);
            minioClientMock.Setup(x => x.WithSSL(It.IsAny<bool>()))
                .Returns(externalMinioClientMock.Object);
            minioClientMock.Setup(x => x.WithTimeout(It.IsAny<int>()))
                .Returns(externalMinioClientMock.Object);
            minioClientMock.Setup(x => x.Build())
                .Returns(externalMinioClientMock.Object);

            // Act
            var result = await sut.GeneratePreSignedHeadUrlAsync(objectKey, 3600);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("localhost:9000", result);
            Assert.Contains("activity-videos", result);

            // Verify that the 403 error was detected and fallback occurred
            loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HEAD request failed with 403 Forbidden")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify that GET-based method was used as fallback
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully generated GET signed URL")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Is403ForbiddenError_ShouldCorrectlyIdentify403Errors()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MinioService>>();
            var minioClientMock = new Mock<IMinioClient>();
            var contextMock = new Mock<ApplicationDbContext>();

            var minioConfig = new MinioConfiguration
            {
                Endpoint = "minio:9000",
                ExternalEndpoint = "localhost:9000",
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                UseSSL = false,
                Region = "us-east-1"
            };

            var sut = new MinioService(minioClientMock.Object, contextMock.Object, minioConfig, loggerMock.Object);

            // Test various 403 error message formats
            var testCases = new[]
            {
                new { Message = "403 Forbidden: Access denied", ShouldBe403 = true },
                new { Message = "403 Forbidden", ShouldBe403 = true },
                new { Message = "Access denied (403)", ShouldBe403 = true },
                new { Message = "Forbidden", ShouldBe403 = true },
                new { Message = "HTTP 403", ShouldBe403 = true },
                new { Message = "404 Not Found", ShouldBe403 = false },
                new { Message = "500 Internal Server Error", ShouldBe403 = false },
                new { Message = "Access denied", ShouldBe403 = false }, // Without 403
                new { Message = "", ShouldBe403 = false }
            };

            foreach (var testCase in testCases)
            {
                // Arrange
                var exception = new Exception(testCase.Message);

                // Act
                var result = sut.Is403ForbiddenError(exception);

                // Assert
                Assert.Equal(testCase.ShouldBe403, result);
            }
        }

        [Fact]
        public async Task Is403ForbiddenError_ShouldCheckInnerExceptions()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MinioService>>();
            var minioClientMock = new Mock<IMinioClient>();
            var contextMock = new Mock<ApplicationDbContext>();

            var minioConfig = new MinioConfiguration
            {
                Endpoint = "minio:9000",
                ExternalEndpoint = "localhost:9000",
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                UseSSL = false,
                Region = "us-east-1"
            };

            var sut = new MinioService(minioClientMock.Object, contextMock.Object, minioConfig, loggerMock.Object);

            // Arrange
            var innerException = new Exception("403 Forbidden: Access denied");
            var outerException = new Exception("Connection failed", innerException);

            // Act
            var result = sut.Is403ForbiddenError(outerException);

            // Assert
            Assert.True(result, "Should check inner exceptions for 403 errors");
        }

        [Fact]
        public async Task GeneratePreSignedHeadUrlAsync_WithNon403Error_ShouldNotFallbackToGet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MinioService>>();
            var minioClientMock = new Mock<IMinioClient>();
            var contextMock = new Mock<ApplicationDbContext>();

            var minioConfig = new MinioConfiguration
            {
                Endpoint = "minio:9000",
                ExternalEndpoint = "localhost:9000",
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                UseSSL = false,
                Region = "us-east-1"
            };

            var sut = new MinioService(minioClientMock.Object, contextMock.Object, minioConfig, loggerMock.Object);
            var objectKey = "videos/activity-999/test-video-4.mp4";

            // Setup mocks
            minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
                .ReturnsAsync(true);

            minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
                .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

            // Create external client mock that throws non-403 error
            var externalMinioClientMock = new Mock<IMinioClient>();
            externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
                .ThrowsAsync(new Exception("Connection timeout"));

            // Setup the MinioClient factory
            minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains("localhost:9000"))))
                .Returns(externalMinioClientMock.Object);
            minioClientMock.Setup(x => x.WithCredentials(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(externalMinioClientMock.Object);
            minioClientMock.Setup(x => x.WithSSL(It.IsAny<bool>()))
                .Returns(externalMinioClientMock.Object);
            minioClientMock.Setup(x => x.WithTimeout(It.IsAny<int>()))
                .Returns(externalMinioClientMock.Object);
            minioClientMock.Setup(x => x.Build())
                .Returns(externalMinioClientMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.GeneratePreSignedHeadUrlAsync(objectKey, 3600));

            // Verify that no fallback to GET occurred (since it wasn't a 403 error)
            loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HEAD request failed with 403 Forbidden")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);

            // Verify that the non-403 error was logged but didn't trigger fallback
            loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to generate HEAD signed URL using external endpoint")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}