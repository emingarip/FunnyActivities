using FluentAssertions;
using FunnyActivities.Infrastructure;
using FunnyActivities.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Minio;
using Minio.DataModel.Args;
using Moq;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Handlers;

public class MinioServiceTests
{
    [Fact]
    public async Task GenerateVideoPreSignedUrlAsync_WithValidObject_ShouldReturnSignedUrl()
    {
        var minioClientMock = CreateMinioClientMock();
        var sut = CreateSut(minioClientMock.Object, externalEndpoint: string.Empty);
        var objectKey = "videos/activity-123/test-video.mp4";
        var expected = "http://minio:9000/activity-videos/videos/activity-123/test-video.mp4?signature=abc";

        minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Minio.DataModel.ObjectStat?)null);
        minioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ReturnsAsync(expected);

        var result = await sut.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GenerateVideoPreSignedUrlAsync_WithInvalidObjectKey_ShouldThrowArgumentException()
    {
        var sut = CreateSut(CreateMinioClientMock().Object, externalEndpoint: string.Empty);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.GenerateVideoPreSignedUrlAsync("", 3600));
    }

    [Fact]
    public async Task GenerateVideoPreSignedUrlAsync_WithInvalidExpiry_ShouldThrowArgumentException()
    {
        var sut = CreateSut(CreateMinioClientMock().Object, externalEndpoint: string.Empty);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.GenerateVideoPreSignedUrlAsync("videos/activity-123/test-video.mp4", -1));
    }

    [Fact]
    public async Task GenerateVideoPreSignedUrlAsync_WithNonExistentObject_ShouldThrowFileNotFoundException()
    {
        var minioClientMock = CreateMinioClientMock();
        var sut = CreateSut(minioClientMock.Object, externalEndpoint: string.Empty);

        minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Object not found"));

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            sut.GenerateVideoPreSignedUrlAsync("videos/activity-123/non-existent-video.mp4", 3600));
    }

    [Fact]
    public async Task GeneratePreSignedHeadUrlAsync_WithInternalFailure_ShouldThrowInvalidOperationException()
    {
        var minioClientMock = CreateMinioClientMock();
        var sut = CreateSut(minioClientMock.Object, externalEndpoint: string.Empty);
        var objectKey = "videos/activity-123/test-video.mp4";

        minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Minio.DataModel.ObjectStat?)null);
        minioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ThrowsAsync(new Exception("Connection timeout"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GeneratePreSignedHeadUrlAsync(objectKey, 3600));
    }

    [Fact]
    public async Task GenerateVideoPreSignedHeadUrlAsync_WithInternalFailure_ShouldThrowInvalidOperationException()
    {
        var minioClientMock = CreateMinioClientMock();
        var sut = CreateSut(minioClientMock.Object, externalEndpoint: string.Empty);
        var objectKey = "videos/activity-456/test-video-2.mp4";

        minioClientMock.Setup(x => x.StatObjectAsync(It.IsAny<StatObjectArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Minio.DataModel.ObjectStat?)null);
        minioClientMock.Setup(x => x.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ThrowsAsync(new Exception("403 Forbidden: Access denied"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GenerateVideoPreSignedHeadUrlAsync(objectKey, 3600));
    }

    private static Mock<IMinioClient> CreateMinioClientMock()
    {
        var minioClientMock = new Mock<IMinioClient>();
        minioClientMock.Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return minioClientMock;
    }

    private static MinioService CreateSut(IMinioClient minioClient, string externalEndpoint)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        var dbContextMock = new Mock<ApplicationDbContext>(options);

        var config = new MinioConfiguration
        {
            Endpoint = "minio:9000",
            ExternalEndpoint = externalEndpoint,
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            UseSSL = false,
            Region = "us-east-1"
        };

        return new MinioService(minioClient, dbContextMock.Object, config, NullLogger<MinioService>.Instance);
    }
}
