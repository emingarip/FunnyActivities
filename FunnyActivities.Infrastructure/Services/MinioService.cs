using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Minio;
using Minio.DataModel.Args;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Enums;
using FunnyActivities.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Infrastructure.Services;

public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly ApplicationDbContext _context;
        private readonly MinioConfiguration _minioConfig;
        private readonly ILogger<MinioService> _logger;
        private const string ProfileImagesBucketName = "profile-images";
        private const string PersonaImagesBucketName = "persona-images";
        private const string ActivityVideosBucketName = "activity-videos";
        private const int MaxRetryAttempts = 3;
        private const int RequestTimeoutSeconds = 30;

        public MinioService(IMinioClient minioClient, ApplicationDbContext context, MinioConfiguration minioConfig, ILogger<MinioService> logger)
        {
            _minioClient = minioClient;
            _context = context;
            _minioConfig = minioConfig;
            _logger = logger;

            // Only ensure buckets exist if MinIO client is available
            if (_minioClient != null)
            {
                EnsureBucketsExistAsync().Wait();
            }
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

            // Ensure persona images bucket exists
            var personaImagesExistsArgs = new BucketExistsArgs().WithBucket(PersonaImagesBucketName);
            bool personaImagesFound = await _minioClient.BucketExistsAsync(personaImagesExistsArgs);

            if (!personaImagesFound)
            {
                var makePersonaImagesBucketArgs = new MakeBucketArgs().WithBucket(PersonaImagesBucketName);
                await _minioClient.MakeBucketAsync(makePersonaImagesBucketArgs);
            }

            // Ensure activity videos bucket exists
            var activityVideosExistsArgs = new BucketExistsArgs().WithBucket(ActivityVideosBucketName);
            bool activityVideosFound = await _minioClient.BucketExistsAsync(activityVideosExistsArgs);

            if (!activityVideosFound)
            {
                var makeActivityVideosBucketArgs = new MakeBucketArgs().WithBucket(ActivityVideosBucketName);
                await _minioClient.MakeBucketAsync(makeActivityVideosBucketArgs);
            }

            // Note: CORS configuration would need to be done manually in MinIO console or via mc CLI
            // The main issue is resolved by using external endpoint for signed URLs
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

        public async Task<string> GeneratePersonaPreSignedUrlAsync(string objectKey, int expiryInSeconds = 3600)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));
            }

            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(PersonaImagesBucketName)
                .WithObject(objectKey)
                .WithExpiry(expiryInSeconds);

            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        }

        public async Task<string> UploadPersonaImageAsync(byte[] imageData, string fileName, string contentType, Guid personaId, string imageType)
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            var objectKey = $"personas/{personaId}/{imageType}/{Guid.NewGuid()}_{sanitizedFileName}";

            using var stream = new MemoryStream(imageData);
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(PersonaImagesBucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
            return objectKey;
        }

        public async Task<string> GeneratePreSignedUrlAsync(string objectKey, int expiryInSeconds = 3600)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));
            }

            if (expiryInSeconds <= 0 || expiryInSeconds > 604800) // Max 7 days
            {
                throw new ArgumentException("Expiry time must be between 1 second and 7 days", nameof(expiryInSeconds));
            }

            // Check if MinIO client is available
            if (_minioClient == null)
            {
                _logger.LogError("MinIO client is not available for object: {ObjectKey}", objectKey);
                throw new InvalidOperationException("MinIO client is not available");
            }

            // Try to extract object key and bucket name from input (handles both object keys and signed URLs)
            // For general objects, we still need to handle both cases
            if (!TryExtractFromInput(objectKey, out var extractedObjectKey, out var bucketName))
            {
                _logger.LogError("Failed to extract object key and bucket name from input: {ObjectKey}", objectKey);
                throw new ArgumentException("Invalid object key or signed URL format", nameof(objectKey));
            }

            // Handle legacy object keys that may be URL-encoded (from before sanitization fix)
            // URL-decode the object key to handle cases where the filename was URL-encoded before storage
            try
            {
                var decodedObjectKey = Uri.UnescapeDataString(extractedObjectKey);
                if (decodedObjectKey != extractedObjectKey)
                {
                    _logger.LogDebug("URL-decoded legacy object key: '{Original}' -> '{Decoded}'", extractedObjectKey, decodedObjectKey);
                    extractedObjectKey = decodedObjectKey;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to URL-decode object key '{ObjectKey}', using original", extractedObjectKey);
            }

            // First check if the object exists
            if (!await ObjectExistsAsync(extractedObjectKey))
            {
                _logger.LogWarning("Object not found: {ObjectKey}", extractedObjectKey);
                throw new FileNotFoundException($"Object not found: {extractedObjectKey}");
            }

            // Validate external endpoint configuration
            if (!await ValidateExternalEndpointConfigurationAsync())
            {
                _logger.LogWarning("External endpoint configuration is invalid or unreachable. Attempting fallback to internal endpoint.");
                return await GeneratePreSignedUrlWithInternalEndpointAsync(extractedObjectKey, expiryInSeconds);
            }

            // Try external endpoint first with retry logic
            try
            {
                return await GeneratePreSignedUrlWithExternalEndpointAsync(extractedObjectKey, expiryInSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate signed URL using external endpoint for object {ObjectKey}. Attempting fallback to internal endpoint.", extractedObjectKey);
                return await GeneratePreSignedUrlWithInternalEndpointAsync(extractedObjectKey, expiryInSeconds);
            }
        }

        public async Task<string> GeneratePreSignedHeadUrlAsync(string objectKey, int expiryInSeconds = 3600)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));
            }

            if (expiryInSeconds <= 0 || expiryInSeconds > 604800) // Max 7 days
            {
                throw new ArgumentException("Expiry time must be between 1 second and 7 days", nameof(expiryInSeconds));
            }

            // Check if MinIO client is available
            if (_minioClient == null)
            {
                _logger.LogError("MinIO client is not available for HEAD request object: {ObjectKey}", objectKey);
                throw new InvalidOperationException("MinIO client is not available");
            }

            // Try to extract object key and bucket name from input (handles both object keys and signed URLs)
            if (!TryExtractFromInput(objectKey, out var extractedObjectKey, out var bucketName))
            {
                _logger.LogError("Failed to extract object key and bucket name from input: {ObjectKey}", objectKey);
                throw new ArgumentException("Invalid object key or signed URL format", nameof(objectKey));
            }

            _logger.LogDebug("Processing HEAD request for object: {ObjectKey} in bucket: {BucketName}", extractedObjectKey, bucketName);

            // First check if the object exists
            if (!await ObjectExistsAsync(extractedObjectKey))
            {
                _logger.LogWarning("Object not found for HEAD request: {ObjectKey}", extractedObjectKey);
                throw new FileNotFoundException($"Object not found: {extractedObjectKey}");
            }

            // Validate external endpoint configuration
            if (!await ValidateExternalEndpointConfigurationAsync())
            {
                _logger.LogWarning("External endpoint configuration is invalid or unreachable. Attempting fallback to internal endpoint for HEAD request.");
                return await GeneratePreSignedHeadUrlWithInternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
            }

            // Try external endpoint first with retry logic
            try
            {
                return await GeneratePreSignedHeadUrlWithExternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
            }
            catch (Exception ex)
            {
                // Check if the error is a 403 Forbidden error
                if (Is403ForbiddenError(ex))
                {
                    _logger.LogWarning("HEAD request failed with 403 Forbidden for object {ObjectKey}. Falling back to GET-based metadata retrieval.", extractedObjectKey);
                    return await GeneratePreSignedGetUrlWithExternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
                }

                _logger.LogWarning(ex, "Failed to generate HEAD signed URL using external endpoint for object {ObjectKey}. Attempting fallback to internal endpoint.", extractedObjectKey);
                try
                {
                    return await GeneratePreSignedHeadUrlWithInternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
                }
                catch (Exception internalEx)
                {
                    // Check if the internal endpoint also fails with 403
                    if (Is403ForbiddenError(internalEx))
                    {
                        _logger.LogWarning("HEAD request failed with 403 Forbidden on internal endpoint for object {ObjectKey}. Falling back to GET-based metadata retrieval.", extractedObjectKey);
                        return await GeneratePreSignedGetUrlWithInternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
                    }
                    throw new InvalidOperationException($"Failed to generate HEAD signed URL for object {extractedObjectKey} using both external and internal endpoints: {internalEx.Message}", internalEx);
                }
            }
        }

        public async Task<bool> DeleteImageAsync(string objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                _logger.LogWarning("Object key is null or empty, cannot delete");
                return false;
            }

            try
            {
                // Try to extract object key and bucket name from input (handles both object keys and signed URLs)
                if (!TryExtractFromInput(objectKey, out var extractedObjectKey, out var bucketName))
                {
                    _logger.LogError("Failed to extract object key and bucket name from input: {ObjectKey}", objectKey);
                    return false;
                }

                _logger.LogDebug("Deleting image object: {ObjectKey} from bucket: {BucketName}", extractedObjectKey, bucketName);

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(extractedObjectKey);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                _logger.LogInformation("Successfully deleted image object: {ObjectKey}", extractedObjectKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image object: {ObjectKey}", objectKey);
                return false;
            }
        }

        public async Task<Image> SaveImageMetadataAsync(Image image)
        {
            await _context.Images.AddAsync(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<string> UploadVideoAsync(byte[] videoData, string fileName, string contentType, Guid activityId, ActivityVideoType videoType = ActivityVideoType.Main)
        {
            // Sanitize filename to prevent double URL encoding issues
            // Remove or replace special characters that cause problems in URLs
            var sanitizedFileName = SanitizeFileName(fileName);
            var folder = videoType == ActivityVideoType.Intro
                ? $"videos/activity-{activityId}/intro"
                : $"videos/activity-{activityId}";
            var objectKey = $"{folder}/{Guid.NewGuid()}_{sanitizedFileName}";

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
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));
            }

            if (expiryInSeconds <= 0 || expiryInSeconds > 604800) // Max 7 days
            {
                throw new ArgumentException("Expiry time must be between 1 second and 7 days", nameof(expiryInSeconds));
            }

            // Check if MinIO client is available
            if (_minioClient == null)
            {
                _logger.LogError("MinIO client is not available for video object: {ObjectKey}", objectKey);
                throw new InvalidOperationException("MinIO client is not available");
            }

            // For video objects, the input should always be an object key for the activity videos bucket
            // Don't try to extract from signed URLs as this can corrupt the object key
            var extractedObjectKey = objectKey;
            var bucketName = ActivityVideosBucketName;

            // Handle legacy object keys that may be URL-encoded (from before sanitization fix)
            // URL-decode the object key to handle cases where the filename was URL-encoded before storage
            try
            {
                var decodedObjectKey = Uri.UnescapeDataString(extractedObjectKey);
                if (decodedObjectKey != extractedObjectKey)
                {
                    _logger.LogDebug("URL-decoded legacy object key: '{Original}' -> '{Decoded}'", extractedObjectKey, decodedObjectKey);
                    extractedObjectKey = decodedObjectKey;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to URL-decode object key '{ObjectKey}', using original", extractedObjectKey);
            }

            _logger.LogDebug("Processing video signed URL request for object: {ObjectKey} in bucket: {BucketName}", extractedObjectKey, bucketName);

            // First check if the video object exists
            if (!await ObjectExistsAsync(extractedObjectKey))
            {
                _logger.LogWarning("Video object not found: {ObjectKey}", extractedObjectKey);
                throw new FileNotFoundException($"Video object not found: {extractedObjectKey}");
            }

            // Validate external endpoint configuration
            if (!await ValidateExternalEndpointConfigurationAsync())
            {
                _logger.LogWarning("External endpoint configuration is invalid or unreachable. Attempting fallback to internal endpoint for video.");
                return await GeneratePreSignedUrlWithInternalEndpointAsync(extractedObjectKey, expiryInSeconds);
            }

            // Try external endpoint first with retry logic
            try
            {
                return await GeneratePreSignedUrlWithExternalEndpointAsync(extractedObjectKey, expiryInSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate video signed URL using external endpoint for object {ObjectKey}. Attempting fallback to internal endpoint.", extractedObjectKey);
                return await GeneratePreSignedUrlWithInternalEndpointAsync(extractedObjectKey, expiryInSeconds);
            }
        }

        /// <summary>
        /// Validates the external endpoint configuration.
        /// </summary>
        private async Task<bool> ValidateExternalEndpointConfigurationAsync()
        {
            try
            {
                // Check if external endpoint is configured
                if (string.IsNullOrWhiteSpace(_minioConfig.ExternalEndpoint))
                {
                    _logger.LogWarning("External endpoint is not configured in MinIO configuration");
                    return false;
                }

                // Validate external endpoint URL format
                if (!Uri.TryCreate(_minioConfig.ExternalEndpoint, UriKind.Absolute, out var externalUri))
                {
                    _logger.LogWarning("External endpoint is not a valid absolute URI: {ExternalEndpoint}", _minioConfig.ExternalEndpoint);
                    return false;
                }

                // Validate that the URI has a valid scheme and host
                if (externalUri.Scheme != Uri.UriSchemeHttp && externalUri.Scheme != Uri.UriSchemeHttps)
                {
                    _logger.LogWarning("External endpoint must use HTTP or HTTPS scheme: {ExternalEndpoint}", _minioConfig.ExternalEndpoint);
                    return false;
                }

                // For localhost endpoints, we can't validate connectivity via HTTP
                // as the MinIO client will handle the actual connection
                if (externalUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    externalUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("External endpoint is localhost-based, skipping connectivity test: {ExternalEndpoint}", _minioConfig.ExternalEndpoint);
                    return true;
                }

                // For non-localhost endpoints, we could test connectivity, but let's keep it simple
                // and let the MinIO client handle connection validation
                _logger.LogDebug("External endpoint configuration is valid: {ExternalEndpoint}", _minioConfig.ExternalEndpoint);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate external endpoint configuration: {ExternalEndpoint}", _minioConfig.ExternalEndpoint);
                return false;
            }
        }

        /// <summary>
        /// Generates a pre-signed URL using the external endpoint with retry logic.
        /// </summary>
        private async Task<string> GeneratePreSignedUrlWithExternalEndpointAsync(string objectKey, int expiryInSeconds)
        {
            var attempt = 0;
            var exceptions = new List<Exception>();

            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    attempt++;
                    _logger.LogDebug("Attempting to generate signed URL using external endpoint (attempt {Attempt}/{MaxAttempts}) for object: {ObjectKey}",
                        attempt, MaxRetryAttempts, objectKey);

                    // For localhost endpoints, use localhost:9000 so frontend can access the URLs
                    var endpointToUse = _minioConfig.ExternalEndpoint;
                    if (endpointToUse.Contains("localhost") || endpointToUse.Contains("127.0.0.1"))
                    {
                        // Use localhost:9000 for external access so frontend can access the signed URLs
                        endpointToUse = "localhost:9000";
                        _logger.LogDebug("Using localhost:9000 for external endpoint access: {Endpoint}", endpointToUse);
                    }

                    // Create a separate MinIO client for external access with timeout
                    var externalMinioClient = new MinioClient()
                        .WithEndpoint(endpointToUse)
                        .WithCredentials(_minioConfig.AccessKey, _minioConfig.SecretKey)
                        .WithSSL(_minioConfig.UseSSL)
                        .WithTimeout(RequestTimeoutSeconds * 1000) // Convert to milliseconds
                        .Build();

                    var presignedGetObjectArgs = new PresignedGetObjectArgs()
                        .WithBucket(ActivityVideosBucketName)
                        .WithObject(objectKey)
                        .WithExpiry(expiryInSeconds);

                    var signedUrl = await externalMinioClient.PresignedGetObjectAsync(presignedGetObjectArgs);

                    _logger.LogInformation("Successfully generated signed URL for object: {ObjectKey} using endpoint: {Endpoint}",
                        objectKey, endpointToUse);
                    return signedUrl;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    _logger.LogWarning(ex, "Attempt {Attempt} failed to generate signed URL using external endpoint for object: {ObjectKey}",
                        attempt, objectKey);

                    if (attempt < MaxRetryAttempts)
                    {
                        // Exponential backoff: 1s, 2s, 4s
                        var delaySeconds = (int)Math.Pow(2, attempt - 1);
                        _logger.LogDebug("Waiting {DelaySeconds} seconds before retry", delaySeconds);
                        await Task.Delay(delaySeconds * 1000);
                    }

                }
            }

            // All attempts failed
            var aggregateException = new AggregateException($"Failed to generate signed URL using external endpoint after {MaxRetryAttempts} attempts", exceptions);
            _logger.LogError(aggregateException, "All attempts failed to generate signed URL using external endpoint for object: {ObjectKey}", objectKey);
            throw aggregateException;
        }

        /// <summary>
        /// Generates a pre-signed URL using the internal endpoint as fallback.
        /// </summary>
        private async Task<string> GeneratePreSignedUrlWithInternalEndpointAsync(string objectKey, int expiryInSeconds)
        {
            try
            {
                _logger.LogInformation("Generating signed URL using internal endpoint as fallback for object: {ObjectKey}", objectKey);

                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(ActivityVideosBucketName)
                    .WithObject(objectKey)
                    .WithExpiry(expiryInSeconds);

                var signedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);

                _logger.LogInformation("Successfully generated signed URL for object: {ObjectKey} using internal endpoint: {InternalEndpoint}",
                    objectKey, _minioConfig.Endpoint);
                return signedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate signed URL using internal endpoint for object: {ObjectKey}", objectKey);
                throw new InvalidOperationException($"Failed to generate signed URL for object {objectKey} using both external and internal endpoints: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a pre-signed HEAD URL using the external endpoint with retry logic.
        /// </summary>
        private async Task<string> GeneratePreSignedHeadUrlWithExternalEndpointAsync(string objectKey, string bucketName, int expiryInSeconds)
        {
            var attempt = 0;
            var exceptions = new List<Exception>();

            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    attempt++;
                    _logger.LogDebug("Attempting to generate HEAD signed URL using external endpoint (attempt {Attempt}/{MaxAttempts}) for object: {ObjectKey}",
                        attempt, MaxRetryAttempts, objectKey);

                    // For localhost endpoints, use localhost:9000 so frontend can access the URLs
                    var endpointToUse = _minioConfig.ExternalEndpoint;
                    if (endpointToUse.Contains("localhost") || endpointToUse.Contains("127.0.0.1"))
                    {
                        // Use localhost:9000 for external access so frontend can access the signed URLs
                        endpointToUse = "localhost:9000";
                        _logger.LogDebug("Using localhost:9000 for external endpoint access: {Endpoint}", endpointToUse);
                    }

                    // Create a separate MinIO client for external access with timeout
                    var externalMinioClient = new MinioClient()
                        .WithEndpoint(endpointToUse)
                        .WithCredentials(_minioConfig.AccessKey, _minioConfig.SecretKey)
                        .WithSSL(_minioConfig.UseSSL)
                        .WithTimeout(RequestTimeoutSeconds * 1000) // Convert to milliseconds
                        .Build();

                    var presignedGetObjectArgs = new PresignedGetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectKey)
                        .WithExpiry(expiryInSeconds);

                    var signedUrl = await externalMinioClient.PresignedGetObjectAsync(presignedGetObjectArgs);

                    _logger.LogInformation("Successfully generated HEAD signed URL for object: {ObjectKey} using endpoint: {Endpoint}",
                        objectKey, endpointToUse);
                    return signedUrl;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    _logger.LogWarning(ex, "Attempt {Attempt} failed to generate HEAD signed URL using external endpoint for object: {ObjectKey}",
                        attempt, objectKey);

                    if (attempt < MaxRetryAttempts)
                    {
                        // Exponential backoff: 1s, 2s, 4s
                        var delaySeconds = (int)Math.Pow(2, attempt - 1);
                        _logger.LogDebug("Waiting {DelaySeconds} seconds before retry", delaySeconds);
                        await Task.Delay(delaySeconds * 1000);
                    }
                }
            }

            // All attempts failed
            var aggregateException = new AggregateException($"Failed to generate HEAD signed URL using external endpoint after {MaxRetryAttempts} attempts", exceptions);
            _logger.LogError(aggregateException, "All attempts failed to generate HEAD signed URL using external endpoint for object: {ObjectKey}", objectKey);
            throw aggregateException;
        }

        /// <summary>
        /// Generates a pre-signed HEAD URL using the internal endpoint as fallback.
        /// </summary>
        private async Task<string> GeneratePreSignedHeadUrlWithInternalEndpointAsync(string objectKey, string bucketName, int expiryInSeconds)
        {
            try
            {
                _logger.LogInformation("Generating HEAD signed URL using internal endpoint as fallback for object: {ObjectKey}", objectKey);

                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey)
                    .WithExpiry(expiryInSeconds);

                var signedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);

                _logger.LogInformation("Successfully generated HEAD signed URL for object: {ObjectKey} using internal endpoint: {InternalEndpoint}",
                    objectKey, _minioConfig.Endpoint);
                return signedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate HEAD signed URL using internal endpoint for object: {ObjectKey}", objectKey);
                throw new InvalidOperationException($"Failed to generate HEAD signed URL for object {objectKey} using both external and internal endpoints: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteVideoAsync(string objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                _logger.LogWarning("Object key is null or empty, cannot delete video");
                return false;
            }

            try
            {
                // Try to extract object key and bucket name from input (handles both object keys and signed URLs)
                if (!TryExtractFromInput(objectKey, out var extractedObjectKey, out var bucketName))
                {
                    _logger.LogError("Failed to extract object key and bucket name from input: {ObjectKey}", objectKey);
                    return false;
                }

                _logger.LogDebug("Deleting video object: {ObjectKey} from bucket: {BucketName}", extractedObjectKey, bucketName);

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(extractedObjectKey);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                _logger.LogInformation("Successfully deleted video object: {ObjectKey}", extractedObjectKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete video object: {ObjectKey}", objectKey);
                return false;
            }
        }

        public async Task<string> GenerateVideoPreSignedHeadUrlAsync(string objectKey, int expiryInSeconds = 3600)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));
            }

            if (expiryInSeconds <= 0 || expiryInSeconds > 604800) // Max 7 days
            {
                throw new ArgumentException("Expiry time must be between 1 second and 7 days", nameof(expiryInSeconds));
            }

            // Check if MinIO client is available
            if (_minioClient == null)
            {
                _logger.LogError("MinIO client is not available for video HEAD request object: {ObjectKey}", objectKey);
                throw new InvalidOperationException("MinIO client is not available");
            }

            // Try to extract object key and bucket name from input (handles both object keys and signed URLs)
            if (!TryExtractFromInput(objectKey, out var extractedObjectKey, out var bucketName))
            {
                _logger.LogError("Failed to extract object key and bucket name from input: {ObjectKey}", objectKey);
                throw new ArgumentException("Invalid object key or signed URL format", nameof(objectKey));
            }

            _logger.LogDebug("Processing video HEAD request for object: {ObjectKey} in bucket: {BucketName}", extractedObjectKey, bucketName);

            // First check if the video object exists
            if (!await ObjectExistsAsync(extractedObjectKey))
            {
                _logger.LogWarning("Video object not found for HEAD request: {ObjectKey}", extractedObjectKey);
                throw new FileNotFoundException($"Video object not found: {extractedObjectKey}");
            }

            // Validate external endpoint configuration
            if (!await ValidateExternalEndpointConfigurationAsync())
            {
                _logger.LogWarning("External endpoint configuration is invalid or unreachable. Attempting fallback to internal endpoint for video HEAD request.");
                return await GeneratePreSignedHeadUrlWithInternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
            }

            // Try external endpoint first with retry logic
            try
            {
                return await GeneratePreSignedHeadUrlWithExternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
            }
            catch (Exception ex)
            {
                // Check if the error is a 403 Forbidden error
                if (Is403ForbiddenError(ex))
                {
                    _logger.LogWarning("Video HEAD request failed with 403 Forbidden for object {ObjectKey}. Falling back to GET-based metadata retrieval.", extractedObjectKey);
                    return await GeneratePreSignedGetUrlWithExternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
                }

                _logger.LogWarning(ex, "Failed to generate video HEAD signed URL using external endpoint for object {ObjectKey}. Attempting fallback to internal endpoint.", extractedObjectKey);
                try
                {
                    return await GeneratePreSignedHeadUrlWithInternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
                }
                catch (Exception internalEx)
                {
                    // Check if the internal endpoint also fails with 403
                    if (Is403ForbiddenError(internalEx))
                    {
                        _logger.LogWarning("Video HEAD request failed with 403 Forbidden on internal endpoint for object {ObjectKey}. Falling back to GET-based metadata retrieval.", extractedObjectKey);
                        return await GeneratePreSignedGetUrlWithInternalEndpointAsync(extractedObjectKey, bucketName, expiryInSeconds);
                    }
                    throw new InvalidOperationException($"Failed to generate video HEAD signed URL for object {extractedObjectKey} using both external and internal endpoints: {internalEx.Message}", internalEx);
                }
            }
        }

        /// <summary>
        /// Retrieves metadata for an object using GET-based requests.
        /// </summary>
        /// <param name="objectKey">The object key or signed URL to retrieve metadata for.</param>
        /// <returns>ObjectMetadata containing the object's metadata information.</returns>
        public async Task<ObjectMetadata> GetObjectMetadataAsync(string objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));
            }

            // Check if MinIO client is available
            if (_minioClient == null)
            {
                _logger.LogError("MinIO client is not available for metadata retrieval: {ObjectKey}", objectKey);
                throw new InvalidOperationException("MinIO client is not available");
            }

            // Try to extract object key and bucket name from input (handles both object keys and signed URLs)
            if (!TryExtractFromInput(objectKey, out var extractedObjectKey, out var bucketName))
            {
                _logger.LogError("Failed to extract object key and bucket name from input: {ObjectKey}", objectKey);
                throw new ArgumentException("Invalid object key or signed URL format", nameof(objectKey));
            }

            _logger.LogDebug("Retrieving metadata for object: {ObjectKey} in bucket: {BucketName}", extractedObjectKey, bucketName);

            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(extractedObjectKey);

                var statResult = await _minioClient.StatObjectAsync(statObjectArgs);

                var metadata = new ObjectMetadata
                {
                    ObjectKey = extractedObjectKey,
                    BucketName = bucketName,
                    Size = statResult.Size,
                    ContentType = statResult.ContentType,
                    LastModified = statResult.LastModified,
                    ETag = statResult.ETag,
                    UserMetadata = new Dictionary<string, string>(statResult.MetaData)
                };

                _logger.LogInformation("Successfully retrieved metadata for object: {ObjectKey}", extractedObjectKey);
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve metadata for object: {ObjectKey}", extractedObjectKey);
                throw new InvalidOperationException($"Failed to retrieve metadata for object {extractedObjectKey}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves metadata for a video object using GET-based requests.
        /// </summary>
        /// <param name="objectKey">The video object key or signed URL to retrieve metadata for.</param>
        /// <returns>ObjectMetadata containing the video object's metadata information.</returns>
        public async Task<ObjectMetadata> GetVideoMetadataAsync(string objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException("Object key cannot be null or empty", nameof(objectKey));
            }

            // Check if MinIO client is available
            if (_minioClient == null)
            {
                _logger.LogError("MinIO client is not available for video metadata retrieval: {ObjectKey}", objectKey);
                throw new InvalidOperationException("MinIO client is not available");
            }

            // Try to extract object key and bucket name from input (handles both object keys and signed URLs)
            if (!TryExtractFromInput(objectKey, out var extractedObjectKey, out var bucketName))
            {
                _logger.LogError("Failed to extract object key and bucket name from input: {ObjectKey}", objectKey);
                throw new ArgumentException("Invalid object key or signed URL format", nameof(objectKey));
            }

            _logger.LogDebug("Retrieving metadata for video object: {ObjectKey} in bucket: {BucketName}", extractedObjectKey, bucketName);

            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(extractedObjectKey);

                var statResult = await _minioClient.StatObjectAsync(statObjectArgs);

                var metadata = new ObjectMetadata
                {
                    ObjectKey = extractedObjectKey,
                    BucketName = bucketName,
                    Size = statResult.Size,
                    ContentType = statResult.ContentType,
                    LastModified = statResult.LastModified,
                    ETag = statResult.ETag,
                    UserMetadata = new Dictionary<string, string>(statResult.MetaData)
                };

                _logger.LogInformation("Successfully retrieved metadata for video object: {ObjectKey}", extractedObjectKey);
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve metadata for video object: {ObjectKey}", extractedObjectKey);
                throw new InvalidOperationException($"Failed to retrieve metadata for video object {extractedObjectKey}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if an object exists in the specified bucket.
        /// </summary>
        /// <param name="objectKey">The object key or signed URL to check.</param>
        /// <returns>True if the object exists, false otherwise.</returns>
        private async Task<bool> ObjectExistsAsync(string objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                _logger.LogWarning("Object key is null or empty, cannot check existence");
                return false;
            }

            try
            {
                // Try to extract object key and bucket name from input (handles both object keys and signed URLs)
                if (!TryExtractFromInput(objectKey, out var extractedObjectKey, out var bucketName))
                {
                    _logger.LogWarning("Failed to extract object key and bucket name from input: {ObjectKey}", objectKey);
                    return false;
                }

                _logger.LogDebug("Checking existence of object: {ObjectKey} in bucket: {BucketName}", extractedObjectKey, bucketName);

                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(extractedObjectKey);

                await _minioClient.StatObjectAsync(statObjectArgs);
                _logger.LogDebug("Object exists: {ObjectKey}", extractedObjectKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Object does not exist or cannot be accessed: {ObjectKey}", objectKey);
                return false;
            }
        }

        /// <summary>
        /// Checks if an exception represents a 403 Forbidden error.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns>True if the exception is a 403 Forbidden error, false otherwise.</returns>
        private bool Is403ForbiddenError(Exception exception)
        {
            // Check if it's a MinIO specific exception with 403 status
            if (exception.Message.Contains("403") || exception.Message.Contains("Forbidden"))
            {
                return true;
            }

            // Check inner exceptions as well
            if (exception.InnerException != null)
            {
                return Is403ForbiddenError(exception.InnerException);
            }

            return false;
        }

        /// <summary>
        /// Tests the signed URL extraction functionality with various URL formats.
        /// This method is for testing purposes and demonstrates the signed URL handling capabilities.
        /// </summary>
        /// <returns>A dictionary containing test results for different signed URL formats.</returns>
        public Dictionary<string, (bool Success, string ObjectKey, string BucketName, string Error)> TestSignedUrlExtraction()
        {
            var testResults = new Dictionary<string, (bool Success, string ObjectKey, string BucketName, string Error)>();

            // Test cases with various signed URL formats
            var testCases = new Dictionary<string, string>
            {
                // Standard MinIO signed URL
                ["Standard MinIO URL"] = "http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature",

                // HTTPS URL
                ["HTTPS URL"] = "https://minio.example.com/profile-images/images/user-456/profile.jpg?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Signature=signature123",

                // URL with different bucket structure
                ["Different Bucket Structure"] = "http://s3.amazonaws.com/my-bucket/path/to/object/file.txt?AWSAccessKeyId=AKIAIOSFODNN7EXAMPLE&Signature=signature&Expires=1234567890",

                // Object key (should not be treated as signed URL)
                ["Object Key Only"] = "videos/activity-789/sample-video.mp4",

                // Invalid URL format
                ["Invalid URL"] = "not-a-valid-url",

                // URL with complex path
                ["Complex Path"] = "http://localhost:9000/activity-videos/videos/activity-999/subfolder/nested/file.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Signature=complex-signature"
            };

            foreach (var testCase in testCases)
            {
                try
                {
                    var success = TryExtractFromInput(testCase.Value, out var objectKey, out var bucketName);
                    testResults[testCase.Key] = (success, objectKey, bucketName, null);
                }
                catch (Exception ex)
                {
                    testResults[testCase.Key] = (false, null, null, ex.Message);
                }
            }

            // Log test results
            foreach (var result in testResults)
            {
                if (result.Value.Success)
                {
                    _logger.LogInformation("Signed URL Test - {TestName}: SUCCESS - ObjectKey: {ObjectKey}, BucketName: {BucketName}",
                        result.Key, result.Value.ObjectKey, result.Value.BucketName);
                }
                else
                {
                    _logger.LogWarning("Signed URL Test - {TestName}: FAILED - {Error}",
                        result.Key, result.Value.Error ?? "Unknown error");
                }
            }

            return testResults;
        }

        /// <summary>
        /// Determines if the input string is a signed URL or an object key.
        /// </summary>
        /// <param name="input">The input string to check.</param>
        /// <returns>True if the input is a signed URL, false if it's an object key.</returns>
        private bool IsSignedUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            // Check if it looks like a URL (contains protocol)
            if (input.StartsWith("http://") || input.StartsWith("https://"))
            {
                return true;
            }

            // Check for MinIO signed URL query parameters
            if (input.Contains("X-Amz-Algorithm") || input.Contains("X-Amz-Signature") || input.Contains("AWSAccessKeyId"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Extracts the object key from a signed URL.
        /// </summary>
        /// <param name="signedUrl">The signed URL to extract the object key from.</param>
        /// <returns>The extracted object key.</returns>
        /// <exception cref="ArgumentException">Thrown when the signed URL format is invalid.</exception>
        private string ExtractObjectKeyFromSignedUrl(string signedUrl)
        {
            if (string.IsNullOrWhiteSpace(signedUrl))
            {
                throw new ArgumentException("Signed URL cannot be null or empty", nameof(signedUrl));
            }

            try
            {
                // Parse the URL
                if (!Uri.TryCreate(signedUrl, UriKind.Absolute, out var uri))
                {
                    throw new ArgumentException("Invalid URL format", nameof(signedUrl));
                }

                // Extract the path part (excluding query parameters)
                var path = uri.AbsolutePath;

                // Remove leading slash if present
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }

                // For MinIO URLs, the format is typically: /bucket-name/object-key
                // So we need to extract the object key part (everything after the bucket name)
                var pathSegments = path.Split('/');

                if (pathSegments.Length < 3) // Need at least: empty, bucket, object-key
                {
                    throw new ArgumentException("Invalid MinIO URL format - missing bucket or object key", nameof(signedUrl));
                }

                // Extract bucket name first
                var bucketName = pathSegments[1]; // pathSegments[0] is empty, [1] is bucket

                // Skip the empty first segment and the bucket name, then join the rest as object key
                var objectKey = string.Join("/", pathSegments.Skip(2));

                if (string.IsNullOrWhiteSpace(objectKey))
                {
                    throw new ArgumentException("Object key could not be extracted from signed URL", nameof(signedUrl));
                }

                // Handle corrupted signed URLs that may have the bucket name duplicated in the object key
                // If the object key starts with the bucket name, strip it to get the correct object key
                if (objectKey.StartsWith(bucketName + "/"))
                {
                    objectKey = objectKey.Substring(bucketName.Length + 1);
                }

                _logger.LogDebug("Successfully extracted object key '{ObjectKey}' from signed URL", objectKey);
                return objectKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract object key from signed URL: {SignedUrl}", signedUrl);
                throw new ArgumentException($"Failed to extract object key from signed URL: {ex.Message}", nameof(signedUrl), ex);
            }
        }

        /// <summary>
        /// Extracts the bucket name from a signed URL.
        /// </summary>
        /// <param name="signedUrl">The signed URL to extract the bucket name from.</param>
        /// <returns>The extracted bucket name.</returns>
        /// <exception cref="ArgumentException">Thrown when the bucket name cannot be extracted.</exception>
        private string ExtractBucketNameFromSignedUrl(string signedUrl)
        {
            if (string.IsNullOrWhiteSpace(signedUrl))
            {
                throw new ArgumentException("Signed URL cannot be null or empty", nameof(signedUrl));
            }

            try
            {
                // Parse the URL
                if (!Uri.TryCreate(signedUrl, UriKind.Absolute, out var uri))
                {
                    throw new ArgumentException("Invalid URL format", nameof(signedUrl));
                }

                // Extract the path part
                var path = uri.AbsolutePath;

                // Remove leading slash if present
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }

                // For MinIO URLs, the format is typically: /bucket-name/object-key
                var pathSegments = path.Split('/');

                if (pathSegments.Length < 1)
                {
                    throw new ArgumentException("Invalid MinIO URL format - missing bucket name", nameof(signedUrl));
                }

                var bucketName = pathSegments[0];

                if (string.IsNullOrWhiteSpace(bucketName))
                {
                    throw new ArgumentException("Bucket name could not be extracted from signed URL", nameof(signedUrl));
                }

                _logger.LogDebug("Successfully extracted bucket name '{BucketName}' from signed URL", bucketName);
                return bucketName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract bucket name from signed URL: {SignedUrl}", signedUrl);
                throw new ArgumentException($"Failed to extract bucket name from signed URL: {ex.Message}", nameof(signedUrl), ex);
            }
        }

        /// <summary>
        /// Attempts to extract object key and bucket name from input, handling both signed URLs and object keys.
        /// Provides robust fallback mechanisms for various URL formats and edge cases.
        /// </summary>
        /// <param name="input">The input string (either signed URL or object key).</param>
        /// <param name="objectKey">The extracted object key.</param>
        /// <param name="bucketName">The extracted bucket name.</param>
        /// <returns>True if extraction was successful, false otherwise.</returns>
        private bool TryExtractFromInput(string input, out string objectKey, out string bucketName)
        {
            objectKey = null;
            bucketName = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                _logger.LogWarning("Input is null or empty, cannot extract object key and bucket name");
                return false;
            }

            try
            {
                if (IsSignedUrl(input))
                {
                    _logger.LogDebug("Input detected as signed URL, extracting object key and bucket name");

                    // Try primary extraction method
                    try
                    {
                        objectKey = ExtractObjectKeyFromSignedUrl(input);
                        bucketName = ExtractBucketNameFromSignedUrl(input);
                        return true;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning(ex, "Primary signed URL extraction failed for input: {Input}, attempting fallback methods", input);

                        // Fallback 1: Try alternative URL parsing
                        if (TryExtractFromSignedUrlWithFallback(input, out objectKey, out bucketName))
                        {
                            _logger.LogDebug("Fallback extraction succeeded for signed URL: {Input}", input);
                            return true;
                        }

                        // Fallback 2: Try to extract just the object key from URL path
                        if (TryExtractObjectKeyFromUrlPath(input, out objectKey))
                        {
                            _logger.LogDebug("Path-based extraction succeeded for signed URL: {Input}", input);
                            // Try to determine bucket from object key pattern
                            bucketName = objectKey.StartsWith("videos/") ? ActivityVideosBucketName : ProfileImagesBucketName;
                            return true;
                        }

                        _logger.LogError("All signed URL extraction methods failed for input: {Input}", input);
                        return false;
                    }
                }
                else
                {
                    // Treat as object key
                    _logger.LogDebug("Input treated as object key: {ObjectKey}", input);

                    // Determine bucket based on object key pattern
                    bucketName = input.StartsWith("videos/") ? ActivityVideosBucketName : ProfileImagesBucketName;
                    objectKey = input;
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract object key and bucket name from input: {Input}", input);
                return false;
            }
        }

        /// <summary>
        /// Attempts alternative extraction methods for signed URLs that failed primary parsing.
        /// </summary>
        private bool TryExtractFromSignedUrlWithFallback(string signedUrl, out string objectKey, out string bucketName)
        {
            objectKey = null;
            bucketName = null;

            try
            {
                // Parse the URL
                if (!Uri.TryCreate(signedUrl, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                // Try to extract bucket from host or path
                var host = uri.Host;
                var path = uri.AbsolutePath;

                // Common MinIO URL patterns:
                // 1. http://minio:9000/bucket-name/object-key
                // 2. http://localhost:9000/bucket-name/object-key
                // 3. http://s3.amazonaws.com/bucket-name/object-key

                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }

                var pathSegments = path.Split('/');

                if (pathSegments.Length >= 2)
                {
                    bucketName = pathSegments[0];
                    objectKey = string.Join("/", pathSegments.Skip(1));

                    if (!string.IsNullOrWhiteSpace(bucketName) && !string.IsNullOrWhiteSpace(objectKey))
                    {
                        _logger.LogDebug("Alternative extraction succeeded - Bucket: {BucketName}, ObjectKey: {ObjectKey}", bucketName, objectKey);
                        return true;
                    }
                }

                // Try to infer bucket from known patterns in object key
                var query = uri.Query;
                if (query.Contains("bucket") || query.Contains("videos") || query.Contains("profile-images"))
                {
                    // This is a last resort - try to extract from query parameters
                    var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                    bucketName = queryParams["bucket"] ?? (objectKey.Contains("videos") ? ActivityVideosBucketName : ProfileImagesBucketName);

                    // Extract object key from path
                    if (pathSegments.Length > 0)
                    {
                        objectKey = string.Join("/", pathSegments);
                    }
                }

                return !string.IsNullOrWhiteSpace(bucketName) && !string.IsNullOrWhiteSpace(objectKey);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Alternative signed URL extraction failed");
                return false;
            }
        }

        /// <summary>
        /// Attempts to extract object key from URL path when other methods fail.
        /// </summary>
        private bool TryExtractObjectKeyFromUrlPath(string url, out string objectKey)
        {
            objectKey = null;

            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                var path = uri.AbsolutePath;
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }

                // Remove common prefixes that might indicate bucket names
                var commonPrefixes = new[] { "activity-videos/", "profile-images/", "videos/", "images/" };

                foreach (var prefix in commonPrefixes)
                {
                    if (path.StartsWith(prefix))
                    {
                        objectKey = path.Substring(prefix.Length);
                        break;
                    }
                }

                // If no prefix matched, assume the whole path is the object key
                if (string.IsNullOrWhiteSpace(objectKey))
                {
                    objectKey = path;
                }

                return !string.IsNullOrWhiteSpace(objectKey);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Path-based object key extraction failed");
                return false;
            }
        }

        /// <summary>
        /// Generates a pre-signed GET URL using the external endpoint with retry logic.
        /// </summary>
        private async Task<string> GeneratePreSignedGetUrlWithExternalEndpointAsync(string objectKey, string bucketName, int expiryInSeconds)
        {
            var attempt = 0;
            var exceptions = new List<Exception>();

            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    attempt++;
                    _logger.LogDebug("Attempting to generate GET signed URL using external endpoint (attempt {Attempt}/{MaxAttempts}) for object: {ObjectKey}",
                        attempt, MaxRetryAttempts, objectKey);

                    // For localhost endpoints, use localhost:9000 so frontend can access the URLs
                    var endpointToUse = _minioConfig.ExternalEndpoint;
                    if (endpointToUse.Contains("localhost") || endpointToUse.Contains("127.0.0.1"))
                    {
                        // Use localhost:9000 for external access so frontend can access the signed URLs
                        endpointToUse = "localhost:9000";
                        _logger.LogDebug("Using localhost:9000 for external endpoint access: {Endpoint}", endpointToUse);
                    }

                    // Create a separate MinIO client for external access with timeout
                    var externalMinioClient = new MinioClient()
                        .WithEndpoint(endpointToUse)
                        .WithCredentials(_minioConfig.AccessKey, _minioConfig.SecretKey)
                        .WithSSL(_minioConfig.UseSSL)
                        .WithTimeout(RequestTimeoutSeconds * 1000) // Convert to milliseconds
                        .Build();

                    var presignedGetObjectArgs = new PresignedGetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectKey)
                        .WithExpiry(expiryInSeconds);

                    var signedUrl = await externalMinioClient.PresignedGetObjectAsync(presignedGetObjectArgs);

                    _logger.LogInformation("Successfully generated GET signed URL for object: {ObjectKey} using endpoint: {Endpoint}",
                        objectKey, endpointToUse);
                    return signedUrl;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    _logger.LogWarning(ex, "Attempt {Attempt} failed to generate GET signed URL using external endpoint for object: {ObjectKey}",
                        attempt, objectKey);

                    if (attempt < MaxRetryAttempts)
                    {
                        // Exponential backoff: 1s, 2s, 4s
                        var delaySeconds = (int)Math.Pow(2, attempt - 1);
                        _logger.LogDebug("Waiting {DelaySeconds} seconds before retry", delaySeconds);
                        await Task.Delay(delaySeconds * 1000);
                    }
                }
            }

            // All attempts failed
            var aggregateException = new AggregateException($"Failed to generate GET signed URL using external endpoint after {MaxRetryAttempts} attempts", exceptions);
            _logger.LogError(aggregateException, "All attempts failed to generate GET signed URL using external endpoint for object: {ObjectKey}", objectKey);
            throw aggregateException;
        }

        /// <summary>
        /// Generates a pre-signed GET URL using the internal endpoint as fallback.
        /// </summary>
        private async Task<string> GeneratePreSignedGetUrlWithInternalEndpointAsync(string objectKey, string bucketName, int expiryInSeconds)
        {
            try
            {
                _logger.LogInformation("Generating GET signed URL using internal endpoint as fallback for object: {ObjectKey}", objectKey);

                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectKey)
                    .WithExpiry(expiryInSeconds);

                var signedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);

                _logger.LogInformation("Successfully generated GET signed URL for object: {ObjectKey} using internal endpoint: {InternalEndpoint}",
                    objectKey, _minioConfig.Endpoint);
                return signedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate GET signed URL using internal endpoint for object: {ObjectKey}", objectKey);
                throw new InvalidOperationException($"Failed to generate GET signed URL for object {objectKey} using both external and internal endpoints: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sanitizes a filename to prevent URL encoding issues and ensure compatibility with MinIO object keys.
        /// </summary>
        /// <param name="fileName">The original filename to sanitize.</param>
        /// <returns>A sanitized filename safe for use in URLs and object keys.</returns>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "unnamed_file";
            }

            try
            {
                // Get the file extension
                var extension = Path.GetExtension(fileName);
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                // Sanitize the name part: remove/replace problematic characters
                // Allow only alphanumeric characters, dots, hyphens, and underscores
                var sanitizedName = System.Text.RegularExpressions.Regex.Replace(nameWithoutExtension, @"[^a-zA-Z0-9._-]", "_");

                // Remove multiple consecutive underscores
                sanitizedName = System.Text.RegularExpressions.Regex.Replace(sanitizedName, @"_+", "_");

                // Trim underscores from start and end
                sanitizedName = sanitizedName.Trim('_');

                // Ensure the name is not empty after sanitization
                if (string.IsNullOrWhiteSpace(sanitizedName))
                {
                    sanitizedName = "file";
                }

                // Limit length to prevent extremely long filenames
                if (sanitizedName.Length > 100)
                {
                    sanitizedName = sanitizedName.Substring(0, 100);
                    sanitizedName = sanitizedName.TrimEnd('_');
                }

                // Reconstruct the filename
                var sanitizedFileName = sanitizedName + extension;

                _logger.LogDebug("Sanitized filename: '{Original}' -> '{Sanitized}'", fileName, sanitizedFileName);
                return sanitizedFileName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sanitize filename '{FileName}', using fallback", fileName);
                return $"file_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(fileName)}";
            }
        }
}
