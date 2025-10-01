using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FunnyActivities.Infrastructure.Services;

namespace MinioServiceTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Testing MinioService localhost:9000 functionality...");

            // Create a mock logger
            var logger = NullLogger<MinioService>.Instance;

            // Create MinIO configuration with localhost external endpoint
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
                // Test 1: Generate signed URL with localhost external endpoint
                Console.WriteLine("\n=== Test 1: Localhost External Endpoint ===");
                var objectKey = "videos/activity-123/test-video.mp4";
                var signedUrl = await minioService.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

                Console.WriteLine($"Object Key: {objectKey}");
                Console.WriteLine($"Generated URL: {signedUrl}");

                // Verify the URL contains localhost:9000
                if (signedUrl.Contains("localhost:9000"))
                {
                    Console.WriteLine("✅ SUCCESS: Signed URL correctly uses localhost:9000");
                }
                else
                {
                    Console.WriteLine("❌ FAILURE: Signed URL does not use localhost:9000");
                }

                // Test 2: Test with 127.0.0.1 external endpoint
                Console.WriteLine("\n=== Test 2: 127.0.0.1 External Endpoint ===");
                var minioConfig127 = new MinioConfiguration
                {
                    Endpoint = "minio:9000",
                    ExternalEndpoint = "127.0.0.1:9000",
                    AccessKey = "test-access-key",
                    SecretKey = "test-secret-key",
                    UseSSL = false,
                    Region = "us-east-1"
                };

                var minioService127 = new MinioService(minioClientMock, contextMock, minioConfig127, logger);
                var signedUrl127 = await minioService127.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

                Console.WriteLine($"Generated URL: {signedUrl127}");

                if (signedUrl127.Contains("localhost:9000"))
                {
                    Console.WriteLine("✅ SUCCESS: 127.0.0.1 endpoint correctly converted to localhost:9000");
                }
                else
                {
                    Console.WriteLine("❌ FAILURE: 127.0.0.1 endpoint not converted to localhost:9000");
                }

                // Test 3: Test with non-localhost external endpoint
                Console.WriteLine("\n=== Test 3: Non-Localhost External Endpoint ===");
                var minioConfigExternal = new MinioConfiguration
                {
                    Endpoint = "minio:9000",
                    ExternalEndpoint = "https://minio.example.com:9000",
                    AccessKey = "test-access-key",
                    SecretKey = "test-secret-key",
                    UseSSL = true,
                    Region = "us-east-1"
                };

                var minioServiceExternal = new MinioService(minioClientMock, contextMock, minioConfigExternal, logger);
                var signedUrlExternal = await minioServiceExternal.GenerateVideoPreSignedUrlAsync(objectKey, 3600);

                Console.WriteLine($"Generated URL: {signedUrlExternal}");

                if (signedUrlExternal.Contains("https://minio.example.com:9000") && !signedUrlExternal.Contains("localhost:9000"))
                {
                    Console.WriteLine("✅ SUCCESS: Non-localhost endpoint preserved correctly");
                }
                else
                {
                    Console.WriteLine("❌ FAILURE: Non-localhost endpoint not handled correctly");
                }

                Console.WriteLine("\n=== Test Summary ===");
                Console.WriteLine("✅ All tests completed. Check the results above to verify localhost:9000 functionality.");

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
            // Return a mock signed URL based on the bucket and object
            var bucket = args.Bucket;
            var objectKey = args.Object;
            var endpoint = "localhost:9000"; // This would be determined by the actual logic

            return Task.FromResult($"http://{endpoint}/{bucket}/{objectKey}?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature");
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
        // Mock response
    }
}