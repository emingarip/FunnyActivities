using System;
using System.Threading.Tasks;
using AutoFixture.AutoMoq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Moq;
using Xunit;
using FunnyActivities.Infrastructure.Services;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace FunnyActivities.Application.UnitTests.Handlers;

public class MinioServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IMinioClient> _minioClientMock;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<MinioService>> _loggerMock;
    private readonly MinioConfiguration _minioConfig;
    private readonly MinioService _sut;

    public MinioServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _minioClientMock = _fixture.Freeze<Mock<IMinioClient>>();
        _contextMock = _fixture.Freeze<Mock<ApplicationDbContext>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<MinioService>>>();

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
    public async Task GenerateVideoPreSignedUrlAsync_WithLocalhostExternalEndpoint_ShouldUseLocalhost9000()
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
        var result = await _sut.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("localhost:9000");
        result.Should().Contain("activity-videos");
        result.Should().Contain(objectKey.Replace("videos/", ""));

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
    public async Task GenerateVideoPreSignedUrlAsync_With127001ExternalEndpoint_ShouldUseLocalhost9000()
    {
        // Arrange
        var objectKey = "videos/activity-456/test-video-2.mp4";
        var expectedUrl = "http://localhost:9000/activity-videos/videos/activity-456/test-video-2.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

        // Setup configuration with 127.0.0.1 external endpoint
        var config127 = new MinioConfiguration
        {
            Endpoint = "minio:9000",
            ExternalEndpoint = "127.0.0.1:9000",
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            UseSSL = false,
            Region = "us-east-1"
        };

        var sut127 = new MinioService(_minioClientMock.Object, _contextMock.Object, config127, _loggerMock.Object);

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
        var result = await sut127.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("localhost:9000");
        result.Should().Contain("activity-videos");
        result.Should().Contain(objectKey.Replace("videos/", ""));

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
    public async Task GenerateVideoPreSignedUrlAsync_WithNonLocalhostExternalEndpoint_ShouldUseOriginalEndpoint()
    {
        // Arrange
        var objectKey = "videos/activity-789/test-video-3.mp4";
        var externalEndpoint = "https://minio.example.com:9000";
        var expectedUrl = $"{externalEndpoint}/activity-videos/videos/activity-789/test-video-3.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

        // Setup configuration with non-localhost external endpoint
        var configExternal = new MinioConfiguration
        {
            Endpoint = "minio:9000",
            ExternalEndpoint = externalEndpoint,
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            UseSSL = true,
            Region = "us-east-1"
        };

        var sutExternal = new MinioService(_minioClientMock.Object, _contextMock.Object, configExternal, _loggerMock.Object);

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
        _minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains(externalEndpoint))))
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
        var result = await sutExternal.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(externalEndpoint);
        result.Should().Contain("activity-videos");
        result.Should().Contain(objectKey.Replace("videos/", ""));

        // Verify that the original external endpoint was used (not localhost:9000)
        result.Should().NotContain("localhost:9000");
    }

    [Fact]
    public async Task GenerateVideoPreSignedUrlAsync_WithInvalidObjectKey_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidObjectKey = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GenerateVideoPreSignedUrlAsync(invalidObjectKey, 3600));
    }

    [Fact]
    public async Task GenerateVideoPreSignedUrlAsync_WithInvalidExpiry_ShouldThrowArgumentException()
    {
        // Arrange
        var objectKey = "videos/activity-123/test-video.mp4";
        var invalidExpiry = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GenerateVideoPreSignedUrlAsync(objectKey, invalidExpiry));
    }

    [Fact]
    public async Task GenerateVideoPreSignedUrlAsync_WithNonExistentObject_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var objectKey = "videos/activity-123/non-existent-video.mp4";

        _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
            .ReturnsAsync(true);

        _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
            .ThrowsAsync(new Exception("Object not found"));

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _sut.GenerateVideoPreSignedUrlAsync(objectKey, 3600));
    }

    [Fact]
    public async Task GeneratePreSignedHeadUrlAsync_With403Error_ShouldFallbackToGetBasedMethod()
    {
        // Arrange
        var objectKey = "videos/activity-123/test-video.mp4";
        var expectedGetUrl = "http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

        // Setup mocks
        _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
            .ReturnsAsync(true);

        _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
            .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

        // Create a separate mock for the external client that throws 403 error
        var externalMinioClientMock = new Mock<IMinioClient>();
        externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ThrowsAsync(new Exception("403 Forbidden: Access denied"));

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
        var result = await _sut.GeneratePreSignedHeadUrlAsync(objectKey, 3600);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("localhost:9000");
        result.Should().Contain("activity-videos");

        // Verify that the 403 error was detected and fallback occurred
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HEAD request failed with 403 Forbidden")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);

        // Verify that GET-based method was used as fallback
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully generated GET signed URL")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GenerateVideoPreSignedHeadUrlAsync_With403Error_ShouldFallbackToGetBasedMethod()
    {
        // Arrange
        var objectKey = "videos/activity-456/test-video-2.mp4";
        var expectedGetUrl = "http://localhost:9000/activity-videos/videos/activity-456/test-video-2.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

        // Setup mocks
        _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
            .ReturnsAsync(true);

        _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
            .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

        // Create a separate mock for the external client that throws 403 error
        var externalMinioClientMock = new Mock<IMinioClient>();
        externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ThrowsAsync(new Exception("403 Forbidden: Access denied"));

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
        var result = await _sut.GenerateVideoPreSignedHeadUrlAsync(objectKey, 3600);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("localhost:9000");
        result.Should().Contain("activity-videos");

        // Verify that the 403 error was detected and fallback occurred
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Video HEAD request failed with 403 Forbidden")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);

        // Verify that GET-based method was used as fallback
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully generated GET signed URL")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GeneratePreSignedHeadUrlAsync_WithCompleteFallbackChain_ShouldWork()
    {
        // Arrange
        var objectKey = "videos/activity-789/test-video-3.mp4";
        var expectedFinalUrl = "http://minio:9000/activity-videos/videos/activity-789/test-video-3.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

        // Setup mocks
        _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
            .ReturnsAsync(true);

        _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
            .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

        // Create external client mock that throws 403
        var externalMinioClientMock = new Mock<IMinioClient>();
        externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ThrowsAsync(new Exception("403 Forbidden: Access denied"));

        // Create internal client mock that also throws 403
        var internalMinioClientMock = new Mock<IMinioClient>();
        internalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ThrowsAsync(new Exception("403 Forbidden: Access denied"));

        // Setup the MinioClient factory to return different mocks based on endpoint
        _minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains("localhost:9000"))))
            .Returns(externalMinioClientMock.Object);
        _minioClientMock.Setup(x => x.WithEndpoint(It.Is<string>(s => s.Contains("minio:9000"))))
            .Returns(internalMinioClientMock.Object);
        _minioClientMock.Setup(x => x.WithCredentials(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((Mock<IMinioClient>)null); // This will be handled by the specific endpoint setup
        _minioClientMock.Setup(x => x.WithSSL(It.IsAny<bool>()))
            .Returns((Mock<IMinioClient>)null);
        _minioClientMock.Setup(x => x.WithTimeout(It.IsAny<int>()))
            .Returns((Mock<IMinioClient>)null);
        _minioClientMock.Setup(x => x.Build())
            .Returns((Mock<IMinioClient>)null);

        // Act
        var result = await _sut.GeneratePreSignedHeadUrlAsync(objectKey, 3600);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("minio:9000");
        result.Should().Contain("activity-videos");

        // Verify that both external and internal HEAD requests failed with 403
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HEAD request failed with 403 Forbidden")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2)); // Both external and internal should fail

        // Verify that final fallback to GET-based method succeeded
        _loggerMock.Verify(x => x.Log(
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
            var result = _sut.Is403ForbiddenError(exception);

            // Assert
            result.Should().Be(testCase.ShouldBe403, $"Exception message '{testCase.Message}' should {(testCase.ShouldBe403 ? "" : "not ")}be identified as 403 error");
        }
    }

    [Fact]
    public async Task Is403ForbiddenError_ShouldCheckInnerExceptions()
    {
        // Arrange
        var innerException = new Exception("403 Forbidden: Access denied");
        var outerException = new Exception("Connection failed", innerException);

        // Act
        var result = _sut.Is403ForbiddenError(outerException);

        // Assert
        result.Should().BeTrue("Should check inner exceptions for 403 errors");
    }

    [Fact]
    public async Task GeneratePreSignedHeadUrlAsync_WithNon403Error_ShouldNotFallbackToGet()
    {
        // Arrange
        var objectKey = "videos/activity-999/test-video-4.mp4";

        // Setup mocks
        _minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>()))
            .ReturnsAsync(true);

        _minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>()))
            .ReturnsAsync(new Minio.DataModel.Response.StatObjectResponse());

        // Create external client mock that throws non-403 error
        var externalMinioClientMock = new Mock<IMinioClient>();
        externalMinioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ThrowsAsync(new Exception("Connection timeout"));

        // Setup the MinioClient factory
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

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.GeneratePreSignedHeadUrlAsync(objectKey, 3600));

        // Verify that no fallback to GET occurred (since it wasn't a 403 error)
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HEAD request failed with 403 Forbidden")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);

        // Verify that the non-403 error was logged but didn't trigger fallback
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to generate HEAD signed URL using external endpoint")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}