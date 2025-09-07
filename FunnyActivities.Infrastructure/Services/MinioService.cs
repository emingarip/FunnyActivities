using System;
using System.IO;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Infrastructure.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly ApplicationDbContext _context;
        private const string ProfileImagesBucketName = "profile-images";
        private const string ActivityVideosBucketName = "activity-videos";

        public MinioService(IMinioClient minioClient, ApplicationDbContext context)
        {
            _minioClient = minioClient;
            _context = context;
            EnsureBucketsExistAsync().Wait();
        }

        private async Task EnsureBucketsExistAsync()
        {
            // Ensure profile images bucket exists
            var profileImagesExistsArgs = new BucketExistsArgs().WithBucket(ProfileImagesBucketName);
            bool profileImagesFound = await _minioClient.BucketExistsAsync(profileImagesExistsArgs);

            if (!profileImagesFound)
            {
                var makeProfileImagesBucketArgs = new MakeBucketArgs().WithBucket(ProfileImagesBucketName);
                await _minioClient.MakeBucketAsync(makeProfileImagesBucketArgs);
            }

            // Ensure activity videos bucket exists
            var activityVideosExistsArgs = new BucketExistsArgs().WithBucket(ActivityVideosBucketName);
            bool activityVideosFound = await _minioClient.BucketExistsAsync(activityVideosExistsArgs);

            if (!activityVideosFound)
            {
                var makeActivityVideosBucketArgs = new MakeBucketArgs().WithBucket(ActivityVideosBucketName);
                await _minioClient.MakeBucketAsync(makeActivityVideosBucketArgs);
            }
        }

        public async Task<string> UploadImageAsync(byte[] imageData, string fileName, string contentType, string imageType)
        {
            var objectKey = $"{imageType}/{Guid.NewGuid()}_{fileName}";

            using var stream = new MemoryStream(imageData);
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(ProfileImagesBucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
            return objectKey;
        }

        public async Task<string> GeneratePreSignedUrlAsync(string objectKey, int expiryInSeconds = 3600)
        {
            // Determine bucket based on object key pattern
            string bucketName = objectKey.StartsWith("videos/") ? ActivityVideosBucketName : ProfileImagesBucketName;

            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithExpiry(expiryInSeconds);

            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        }

        public async Task<bool> DeleteImageAsync(string objectKey)
        {
            try
            {
                // Determine bucket based on object key pattern
                string bucketName = objectKey.StartsWith("videos/") ? ActivityVideosBucketName : ProfileImagesBucketName;

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Image> SaveImageMetadataAsync(Image image)
        {
            await _context.Images.AddAsync(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<string> UploadVideoAsync(byte[] videoData, string fileName, string contentType, Guid activityId)
        {
            var objectKey = $"videos/activity-{activityId}/{Guid.NewGuid()}_{fileName}";

            using var stream = new MemoryStream(videoData);
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(ActivityVideosBucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
            return objectKey;
        }

        public async Task<string> GenerateVideoPreSignedUrlAsync(string objectKey, int expiryInSeconds = 3600)
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(ActivityVideosBucketName)
                .WithObject(objectKey)
                .WithExpiry(expiryInSeconds);

            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        }

        public async Task<bool> DeleteVideoAsync(string objectKey)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(ActivityVideosBucketName)
                    .WithObject(objectKey);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}