using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Simple403DetectionTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Testing 403 error detection logic...");

            // Test the Is403ForbiddenError method logic
            Test403Detection();

            Console.WriteLine("All tests completed.");
        }

        private static void Test403Detection()
        {
            Console.WriteLine("\n=== Testing 403 Error Detection ===");

            // Test case 1: Direct 403 message
            var ex1 = new Exception("403 Forbidden");
            bool result1 = Is403ForbiddenError(ex1);
            Console.WriteLine($"Test 1 - Direct 403 message: {result1} (expected: true)");
            Console.WriteLine($"  Exception: {ex1.Message}");

            // Test case 2: "Forbidden" message
            var ex2 = new Exception("Forbidden");
            bool result2 = Is403ForbiddenError(ex2);
            Console.WriteLine($"Test 2 - Forbidden message: {result2} (expected: true)");
            Console.WriteLine($"  Exception: {ex2.Message}");

            // Test case 3: Inner exception with 403
            var innerEx = new Exception("403 Forbidden");
            var ex3 = new Exception("Request failed", innerEx);
            bool result3 = Is403ForbiddenError(ex3);
            Console.WriteLine($"Test 3 - Inner exception with 403: {result3} (expected: true)");
            Console.WriteLine($"  Exception: {ex3.Message}, Inner: {ex3.InnerException?.Message}");

            // Test case 4: Non-403 error
            var ex4 = new Exception("404 Not Found");
            bool result4 = Is403ForbiddenError(ex4);
            Console.WriteLine($"Test 4 - Non-403 error: {result4} (expected: false)");
            Console.WriteLine($"  Exception: {ex4.Message}");

            // Test case 5: Null exception
            bool result5 = Is403ForbiddenError(null);
            Console.WriteLine($"Test 5 - Null exception: {result5} (expected: false)");

            // Test case 6: Empty message
            var ex6 = new Exception("");
            bool result6 = Is403ForbiddenError(ex6);
            Console.WriteLine($"Test 6 - Empty message: {result6} (expected: false)");
            Console.WriteLine($"  Exception: '{ex6.Message}'");

            // Test case 7: Case insensitive
            var ex7 = new Exception("forbidden");
            bool result7 = Is403ForbiddenError(ex7);
            Console.WriteLine($"Test 7 - Case insensitive forbidden: {result7} (expected: true)");
            Console.WriteLine($"  Exception: {ex7.Message}");

            // Test case 8: Nested inner exceptions
            var innerInnerEx = new Exception("403 Forbidden");
            var innerEx2 = new Exception("Connection failed", innerInnerEx);
            var ex8 = new Exception("Request timeout", innerEx2);
            bool result8 = Is403ForbiddenError(ex8);
            Console.WriteLine($"Test 8 - Nested inner exceptions: {result8} (expected: true)");
            Console.WriteLine($"  Exception: {ex8.Message}, Inner: {ex8.InnerException?.Message}, InnerInner: {ex8.InnerException?.InnerException?.Message}");
        }

        private static bool Is403ForbiddenError(Exception ex)
        {
            if (ex == null)
                return false;

            // Check current exception message
            if (!string.IsNullOrEmpty(ex.Message) &&
                (ex.Message.Contains("403") || ex.Message.Contains("Forbidden")))
            {
                return true;
            }

            // Check inner exception recursively
            return Is403ForbiddenError(ex.InnerException);
        }
    }
}