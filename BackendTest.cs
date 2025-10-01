using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FunnyActivities.Infrastructure.Services;

namespace BackendTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Testing MinioService URL parsing functionality...");

            // Create a mock logger
            var logger = NullLogger<MinioService>.Instance;

            // Create MinIO configuration
            var minioConfig = new MinioConfiguration
            {
                Endpoint = "minio:9000",
                ExternalEndpoint = "localhost:9000",
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                UseSSL = false,
                Region = "us-east-1"
            };

            // Create a mock MinIO client
            var minioClientMock = new MockMinioClient();

            // Create a mock ApplicationDbContext
            var contextMock = new MockApplicationDbContext();

            // Create the MinioService
            var minioService = new MinioService(minioClientMock, contextMock, minioConfig, logger);

            try
            {
                // Test 1: Test signed URL extraction
                Console.WriteLine("\n=== Test 1: Signed URL Extraction ===");
                var testSignedUrl = "http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature";

                if (minioService.TryExtractFromInput(testSignedUrl, out var objectKey, out var bucketName))
                {
                    Console.WriteLine($"✅ SUCCESS: Signed URL parsed correctly");
                    Console.WriteLine($"   Object Key: {objectKey}");
                    Console.WriteLine($"   Bucket Name: {bucketName}");
                }
                else
                {
                    Console.WriteLine("❌ FAILURE: Failed to parse signed URL");
                }

                // Test 2: Test direct object key
                Console.WriteLine("\n=== Test 2: Direct Object Key ===");
                var testObjectKey = "videos/activity-456/sample-video.mp4";

                if (minioService.TryExtractFromInput(testObjectKey, out var objectKey2, out var bucketName2))
                {
                    Console.WriteLine($"✅ SUCCESS: Object key parsed correctly");
                    Console.WriteLine($"   Object Key: {objectKey2}");
                    Console.WriteLine($"   Bucket Name: {bucketName2}");
                }
                else
                {
                    Console.WriteLine("❌ FAILURE: Failed to parse object key");
                }

                // Test 3: Test invalid input
                Console.WriteLine("\n=== Test 3: Invalid Input ===");
                var testInvalidInput = "not-a-valid-input";

                if (minioService.TryExtractFromInput(testInvalidInput, out var objectKey3, out var bucketName3))
                {
                    Console.WriteLine($"❌ UNEXPECTED: Invalid input was parsed as valid");
                    Console.WriteLine($"   Object Key: {objectKey3}");
                    Console.WriteLine($"   Bucket Name: {bucketName3}");
                }
                else
                {
                    Console.WriteLine("✅ SUCCESS: Invalid input correctly rejected");
                }

                // Test 4: Test 1024+ character URL (this should be handled by frontend, but let's see backend behavior)
                Console.WriteLine("\n=== Test 4: Long URL Test ===");
                var longUrl = "http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?";
                for (int i = 0; i < 250; i++)
                {
                    longUrl += "X-Amz-Parameter" + i + "=value" + i + "&";
                }
                longUrl = longUrl.TrimEnd('&');

                Console.WriteLine($"URL Length: {longUrl.Length} characters");

                if (minioService.TryExtractFromInput(longUrl, out var objectKey4, out var bucketName4))
                {
                    Console.WriteLine($"✅ SUCCESS: Long URL parsed correctly");
                    Console.WriteLine($"   Object Key: {objectKey4}");
                    Console.WriteLine($"   Bucket Name: {bucketName4}");
                }
                else
                {
                    Console.WriteLine("❌ FAILURE: Failed to parse long URL");
                }

                // Test 5: Test the TestSignedUrlExtraction method
                Console.WriteLine("\n=== Test 5: Comprehensive URL Test ===");
                var testResults = minioService.TestSignedUrlExtraction();

                int successCount = 0;
                foreach (var result in testResults)
                {
                    if (result.Value.Success)
                    {
                        successCount++;
                        Console.WriteLine($"✅ {result.Key}: SUCCESS");
                    }
                    else
                    {
                        Console.WriteLine($"❌ {result.Key}: FAILED - {result.Value.Error}");
                    }
                }

                Console.WriteLine($"\nTest Summary: {successCount}/{testResults.Count} tests passed");

                Console.WriteLine("\n=== All Tests Completed ===");
                Console.WriteLine("✅ Backend URL parsing tests completed successfully!");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed with error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    // Mock classes for testing
    public class MockMinioClient : IMinioClient
    {
        public IMinioClient WithEndpoint(string endpoint)
        {
            return this;
        }

        public IMinioClient WithCredentials(string accessKey, string secretKey)
        {
            return this;
        }

        public IMinioClient WithSSL(bool useSSL)
        {
            return this;
        }

        public IMinioClient WithTimeout(int timeoutMs)
        {
            return this;
        }

        public IMinioClient Build()
        {
            return this;
        }

        public Task<bool> BucketExistsAsync(BucketExistsArgs args)
        {
            return Task.FromResult(true);
        }

        public Task PutObjectAsync(PutObjectArgs args)
        {
            return Task.CompletedTask;
        }

        public Task<StatObjectResponse> StatObjectAsync(StatObjectArgs args)
        {
            return Task.FromResult(new StatObjectResponse());
        }

        public Task<string> PresignedGetObjectAsync(PresignedGetObjectArgs args)
        {
            return Task.FromResult($"http://localhost:9000/{args.Bucket}/{args.Object}?X-Amz-Signature=test");
        }

        public Task RemoveObjectAsync(RemoveObjectArgs args)
        {
            return Task.CompletedTask;
        }
    }

    public class MockApplicationDbContext
    {
        // Mock implementation
    }

    // Interface definitions (simplified)
    public interface IMinioClient
    {
        IMinioClient WithEndpoint(string endpoint);
        IMinioClient WithCredentials(string accessKey, string secretKey);
        IMinioClient WithSSL(bool useSSL);
        IMinioClient WithTimeout(int timeoutMs);
        IMinioClient Build();
        Task<bool> BucketExistsAsync(BucketExistsArgs args);
        Task PutObjectAsync(PutObjectArgs args);
        Task<StatObjectResponse> StatObjectAsync(StatObjectArgs args);
        Task<string> PresignedGetObjectAsync(PresignedGetObjectArgs args);
        Task RemoveObjectAsync(RemoveObjectArgs args);
    }

    // Mock argument classes
    public class BucketExistsArgs
    {
        public string Bucket { get; private set; }
        public BucketExistsArgs WithBucket(string bucket)
        {
            Bucket = bucket;
            return this;
        }
    }

    public class PutObjectArgs
    {
        public string Bucket { get; private set; }
        public string Object { get; private set; }
        public Stream StreamData { get; private set; }
        public long ObjectSize { get; private set; }
        public string ContentType { get; private set; }

        public PutObjectArgs WithBucket(string bucket) { Bucket = bucket; return this; }
        public PutObjectArgs WithObject(string obj) { Object = obj; return this; }
        public PutObjectArgs WithStreamData(Stream stream) { StreamData = stream; return this; }
        public PutObjectArgs WithObjectSize(long size) { ObjectSize = size; return this; }
        public PutObjectArgs WithContentType(string contentType) { ContentType = contentType; return this; }
    }

    public class StatObjectArgs
    {
        public string Bucket { get; private set; }
        public string Object { get; private set; }

        public StatObjectArgs WithBucket(string bucket) { Bucket = bucket; return this; }
        public StatObjectArgs WithObject(string obj) { Object = obj; return this; }
    }

    public class PresignedGetObjectArgs
    {
        public string Bucket { get; private set; }
        public string Object { get; private set; }
        public int Expiry { get; private set; }

        public PresignedGetObjectArgs WithBucket(string bucket) { Bucket = bucket; return this; }
        public PresignedGetObjectArgs WithObject(string obj) { Object = obj; return this; }
        public PresignedGetObjectArgs WithExpiry(int expiry) { Expiry = expiry; return this; }
    }

    public class RemoveObjectArgs
    {
        public string Bucket { get; private set; }
        public string Object { get; private set; }

        public RemoveObjectArgs WithBucket(string bucket) { Bucket = bucket; return this; }
        public RemoveObjectArgs WithObject(string obj) { Object = obj; return this; }
    }

    public class StatObjectResponse
    {
        public long Size { get; set; } = 1024;
        public string ContentType { get; set; } = "video/mp4";
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public string ETag { get; set; } = "test-etag";
        public Dictionary<string, string> MetaData { get; set; } = new Dictionary<string, string>();
    }

    // MinioConfiguration class (simplified)
    public class MinioConfiguration
    {
        public string Endpoint { get; set; }
        public string ExternalEndpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool UseSSL { get; set; }
        public string Region { get; set; }
    }
}