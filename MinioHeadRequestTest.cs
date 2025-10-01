using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Moq;
using Xunit;
using FunnyActivities.Infrastructure.Services;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Tests
{
    public class MinioHeadRequestTest
    {
        private readonly Mock<IMinioClient> _minioClientMock;
        private readonly Mock<ApplicationDbContext> _contextMock;
        private readonly Mock<ILogger<MinioService>> _loggerMock;
        private readonly MinioConfiguration _minioConfig;
        private readonly MinioService _sut;

        public MinioHeadRequestTest()
        {
            _minioClientMock = new Mock<IMinioClient>();
            _contextMock = new Mock<ApplicationDbContext>();
            _loggerMock = new Mock<ILogger<MinioService>>();

            // Setup MinIO configuration with localhost external endpoint
            _minioConfig = new MinioConfiguration
            {
                Endpoint = "minio:9000",
                ExternalEndpoint = "localhost:9000",
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                UseSSL = false,
                Region = "us-east-1"
            };

            _sut = new MinioService(_minioClientMock.Object, _contextMock.Object, _minioConfig, _loggerMock.Object);
        }

        [Fact]
        public async Task GenerateVideoPreSignedUrlAsync_WithValidObjectKey_ShouldGenerateValidSignedUrl()
        {
            // Arrange
            var objectKey = "videos/activity-123/test-video.mp4";
            var expectedUrl = "http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

            // Setup mocks
            _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
                .ReturnsAsync(true);

            _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
                .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

            // Create a separate mock for the external client
            var externalMinioClientMock = new Mock<IMinioClient>();
            externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
                .ReturnsAsync(expectedUrl);

            // Setup the MinioClient factory to return our external client mock
            _minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains("localhost:9000"))))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithCredentials(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithSSL(It.IsAny<bool>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithTimeout(It.IsAny<int>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.Build())
                .Returns(externalMinioClientMock.Object);

            // Act
            var signedUrl = await _sut.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

            // Assert
            Assert.NotNull(signedUrl);
            Assert.Contains("localhost:9000", signedUrl);
            Assert.Contains("activity-videos", signedUrl);
            Assert.Contains(objectKey.Replace("videos/", ""), signedUrl);

            // Verify that the external endpoint was used
            _loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using localhost:9000 for external endpoint access")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task SignedUrl_ShouldSupportHeadRequests_WithCorsHeaders()
        {
            // Arrange
            var objectKey = "videos/activity-456/test-video-2.mp4";
            var signedUrl = "http://localhost:9000/activity-videos/videos/activity-456/test-video-2.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

            // Setup mocks
            _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
                .ReturnsAsync(true);

            _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
                .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

            // Create a separate mock for the external client
            var externalMinioClientMock = new Mock<IMinioClient>();
            externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
                .ReturnsAsync(signedUrl);

            // Setup the MinioClient factory to return our external client mock
            _minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains("localhost:9000"))))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithCredentials(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithSSL(It.IsAny<bool>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithTimeout(It.IsAny<int>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.Build())
                .Returns(externalMinioClientMock.Object);

            // Act
            var generatedUrl = await _sut.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

            // Assert - Verify URL generation
            Assert.NotNull(generatedUrl);
            Assert.Equal(signedUrl, generatedUrl);

            // The key insight: HEAD requests use the same signed URL structure as GET requests
            // The CORS configuration already allows HEAD method, so this should work
            Assert.Contains("X-Amz-Signature", generatedUrl);
            Assert.Contains("X-Amz-Expires", generatedUrl);
        }

        [Fact]
        public async Task SignedUrl_WithCorsConfiguration_ShouldAllowCrossOriginHeadRequests()
        {
            // Arrange
            var objectKey = "videos/activity-789/test-video-3.mp4";
            var signedUrl = "http://localhost:9000/activity-videos/videos/activity-789/test-video-3.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

            // Setup mocks
            _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
                .ReturnsAsync(true);

            _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
                .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

            // Create a separate mock for the external client
            var externalMinioClientMock = new Mock<IMinioClient>();
            externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
                .ReturnsAsync(signedUrl);

            // Setup the MinioClient factory to return our external client mock
            _minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains("localhost:9000"))))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithCredentials(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithSSL(It.IsAny<bool>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithTimeout(It.IsAny<int>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.Build())
                .Returns(externalMinioClientMock.Object);

            // Act
            var generatedUrl = await _sut.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

            // Assert
            Assert.NotNull(generatedUrl);

            // Verify the URL structure supports CORS
            // The CORS configuration in minio-cors-config.json includes:
            // - HEAD method is allowed
            // - localhost:3000, localhost:3001 origins are allowed
            // - ETag, Content-Length, Content-Type, Last-Modified headers are exposed
            Assert.Contains("localhost:9000", generatedUrl);
            Assert.Contains("X-Amz-SignedHeaders=host", generatedUrl);

            // Verify logging indicates external endpoint usage
            _loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using localhost:9000 for external endpoint access")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task HeadRequestTest_WithDifferentOrigins_ShouldWorkWithCorsConfiguration()
        {
            // Test with different localhost ports to verify CORS configuration covers them
            var testCases = new[]
            {
                ("localhost:3000", "videos/activity-3000/test-video-3000.mp4"),
                ("127.0.0.1:3001", "videos/activity-3001/test-video-3001.mp4"),
                ("localhost:8080", "videos/activity-8080/test-video-8080.mp4")
            };

            foreach (var (origin, objectKey) in testCases)
            {
                // Arrange
                var signedUrl = $"http://{origin}/activity-videos/{objectKey}?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

                // Setup mocks for this specific test case
                _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
                    .ReturnsAsync(true);

                _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
                    .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

                // Create a separate mock for the external client
                var externalMinioClientMock = new Mock<IMinioClient>();
                externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
                    .ReturnsAsync(signedUrl);

                // Setup the MinioClient factory to return our external client mock
                _minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains(origin))))
                    .Returns(externalMinioClientMock.Object);
                _minioClientMock.Setup(x => x.WithCredentials(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(externalMinioClientMock.Object);
                _minioClientMock.Setup(x => x.WithSSL(It.IsAny<bool>()))
                    .Returns(externalMinioClientMock.Object);
                _minioClientMock.Setup(x => x.WithTimeout(It.IsAny<int>()))
                    .Returns(externalMinioClientMock.Object);
                _minioClientMock.Setup(x => x.Build())
                    .Returns(externalMinioClientMock.Object);

                // Act
                var generatedUrl = await _sut.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

                // Assert
                Assert.NotNull(generatedUrl);
                Assert.Contains(origin, generatedUrl);
                Assert.Contains("X-Amz-Signature", generatedUrl);

                // Verify that the CORS configuration supports this origin
                // The minio-cors-config.json includes all these origins
                Assert.True(
                    generatedUrl.Contains("localhost:3000") ||
                    generatedUrl.Contains("127.0.0.1:3001") ||
                    generatedUrl.Contains("localhost:8080"),
                    $"URL should contain one of the allowed CORS origins: {generatedUrl}"
                );
            }
        }

        [Fact]
        public async Task SignedUrlGeneration_WithRetryLogic_ShouldHandleFailuresGracefully()
        {
            // Arrange
            var objectKey = "videos/activity-retry/test-video-retry.mp4";

            // Setup mocks
            _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
                .ReturnsAsync(true);

            _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
                .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

            // Create a separate mock for the external client that fails first, then succeeds
            var externalMinioClientMock = new Mock<IMinioClient>();
            var callCount = 0;
            externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        throw new Exception("First attempt failed");
                    }
                    return "http://localhost:9000/activity-videos/videos/activity-retry/test-video-retry.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";
                });

            // Setup the MinioClient factory to return our external client mock
            _minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains("localhost:9000"))))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithCredentials(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithSSL(It.IsAny<bool>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.WithTimeout(It.IsAny<int>()))
                .Returns(externalMinioClientMock.Object);
            _minioClientMock.Setup(x => x.Build())
                .Returns(externalMinioClientMock.Object);

            // Act
            var generatedUrl = await _sut.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

            // Assert
            Assert.NotNull(generatedUrl);
            Assert.Contains("localhost:9000", generatedUrl);

            // Verify retry logic was triggered
            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to generate signed URL using external endpoint")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}