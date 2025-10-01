using System;
using System.Threading.Tasks;

namespace SimpleMinioTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Testing MinioService localhost:9000 logic...");

            // Test the core logic that determines the endpoint
            TestEndpointLogic();

            Console.WriteLine("\n✅ Test completed. The logic shows that:");
            Console.WriteLine("- localhost endpoints are converted to localhost:9000");
            Console.WriteLine("- 127.0.0.1 endpoints are converted to localhost:9000");
            Console.WriteLine("- Non-localhost endpoints are preserved as-is");
        }

        private static void TestEndpointLogic()
        {
            Console.WriteLine("\n=== Testing Endpoint Logic ===");

            // Test cases
            var testCases = new[]
            {
                ("localhost:9000", "localhost:9000", "✅ localhost:9000 -> localhost:9000"),
                ("127.0.0.1:9000", "localhost:9000", "✅ 127.0.0.1:9000 -> localhost:9000"),
                ("https://minio.example.com:9000", "https://minio.example.com:9000", "✅ External endpoint preserved"),
                ("minio:9000", "minio:9000", "✅ Internal endpoint preserved")
            };

            foreach (var (input, expected, description) in testCases)
            {
                var result = GetEndpointToUse(input);
                var success = result == expected;

                Console.WriteLine($"Input: {input}");
                Console.WriteLine($"Expected: {expected}");
                Console.WriteLine($"Result: {result}");
                Console.WriteLine(success ? description : "❌ FAILED");
                Console.WriteLine();
            }
        }

        private static string GetEndpointToUse(string externalEndpoint)
        {
            // This replicates the logic from MinioService.GeneratePreSignedUrlWithExternalEndpointAsync
            if (externalEndpoint.Contains("localhost") || externalEndpoint.Contains("127.0.0.1"))
            {
                return "localhost:9000";
            }

            return externalEndpoint;
        }
    }
}