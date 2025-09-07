using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;
using FunnyActivities.CrossCuttingConcerns.FileUpload;
using FunnyActivities.CrossCuttingConcerns.FileUpload.Exceptions;
using FunnyActivities.CrossCuttingConcerns.FileUpload.Configuration;

namespace FunnyActivities.Infrastructure.Services
{
    public class MinioStorageProvider : IStorageProvider
    {
        private readonly IMinioClient _minioClient;
        private readonly FileUploadConfiguration _configuration;

        public MinioStorageProvider(IMinioClient minioClient, FileUploadConfiguration configuration)
        {
            _minioClient = minioClient;
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(string bucketName, string objectKey, byte[] fileData, string contentType, Dictionary<string, string> metadata = null)
        {
            try
            {
                // Ensure bucket exists
                await EnsureBucketExistsAsync(bucketName);

                using var stream = new MemoryStream(fileData);
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType);

                if (metadata != null && metadata.Count > 0)
                {
                    // Convert metadata to MinIO format
                    var headers = new Dictionary<string, string>();
                    foreach (var kvp in metadata)
                    {
                        headers[$"x-amz-meta-{kvp.Key}"] = kvp.Value;
                    }
                    putObjectArgs = putObjectArgs.WithHeaders(headers);
                }

                await _minioClient.PutObjectAsync(putObjectArgs);
                return objectKey;
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to upload file to bucket '{bucketName}' with key '{objectKey}'", "Upload", bucketName, objectKey, ex);
            }
        }

        public async Task<byte[]> DownloadFileAsync(string bucketName, string objectKey)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey)
                    .WithCallbackStream(stream =>
                    {
                        stream.CopyTo(memoryStream);
                    });

                await _minioClient.GetObjectAsync(getObjectArgs);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to download file from bucket '{bucketName}' with key '{objectKey}'", "Download", bucketName, objectKey, ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string bucketName, string objectKey)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                return true;
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to delete file from bucket '{bucketName}' with key '{objectKey}'", "Delete", bucketName, objectKey, ex);
            }
        }

        public async Task<string> GeneratePreSignedUrlAsync(string bucketName, string objectKey, int expiryInSeconds = 3600)
        {
            try
            {
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey)
                    .WithExpiry(expiryInSeconds);

                return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to generate pre-signed URL for bucket '{bucketName}' with key '{objectKey}'", "GeneratePreSignedUrl", bucketName, objectKey, ex);
            }
        }

        public async Task<bool> FileExistsAsync(string bucketName, string objectKey)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey);

                await _minioClient.StatObjectAsync(statObjectArgs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<FileMetadata> GetFileMetadataAsync(string bucketName, string objectKey)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey);

                var objectStat = await _minioClient.StatObjectAsync(statObjectArgs);

                return new FileMetadata
                {
                    FileId = objectKey,
                    FileName = Path.GetFileName(objectKey),
                    ContentType = objectStat.ContentType,
                    FileSize = objectStat.Size,
                    BucketName = bucketName,
                    UploadedAt = objectStat.LastModified
                };
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to get metadata for file in bucket '{bucketName}' with key '{objectKey}'", "GetMetadata", bucketName, objectKey, ex);
            }
        }

        public async Task<List<string>> ListFilesAsync(string bucketName, string prefix = null)
        {
            try
            {
                var files = new List<string>();
                var listObjectsArgs = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix(prefix ?? "")
                    .WithRecursive(true);

                var observable = _minioClient.ListObjectsAsync(listObjectsArgs);
                var tcs = new TaskCompletionSource<bool>();

                observable.Subscribe(
                    item => files.Add(item.Key),
                    ex => tcs.SetException(ex),
                    () => tcs.SetResult(true)
                );

                await tcs.Task;
                return files;
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to list files in bucket '{bucketName}'", "ListFiles", bucketName, "", ex);
            }
        }

        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            try
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
                return true;
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to create bucket '{bucketName}'", "CreateBucket", bucketName, "", ex);
            }
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
                return await _minioClient.BucketExistsAsync(bucketExistsArgs);
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to check if bucket '{bucketName}' exists", "BucketExists", bucketName, "", ex);
            }
        }

        public async Task<bool> DeleteBucketAsync(string bucketName)
        {
            try
            {
                var removeBucketArgs = new RemoveBucketArgs().WithBucket(bucketName);
                await _minioClient.RemoveBucketAsync(removeBucketArgs);
                return true;
            }
            catch (Exception ex)
            {
                throw new StorageException($"Failed to delete bucket '{bucketName}'", "DeleteBucket", bucketName, "", ex);
            }
        }

        private async Task EnsureBucketExistsAsync(string bucketName)
        {
            if (!await BucketExistsAsync(bucketName))
            {
                await CreateBucketAsync(bucketName);
            }
        }
    }
}