using System;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.ValueObjects;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Domain.Services
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
        }

        public bool IsValidEmail(Email email)
        {
            // Simple validation, in real app use regex
            return email.Value.Contains("@");
        }

        public string HashPassword(Password password)
        {
            _logger.LogDebug("[USER-SERVICE] Starting password hashing operation");

            var startTime = DateTime.UtcNow;
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password.Value);
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogInformation("[USER-SERVICE] Password hashing completed in {Duration}ms. Hash length: {HashLength}",
                duration.TotalMilliseconds, hashedPassword?.Length ?? 0);

            return hashedPassword;
        }

        public bool VerifyPassword(string hashedPassword, string plainPassword)
        {
            _logger.LogDebug("[USER-SERVICE] Starting password verification. Hash format check: {IsOldFormat}",
                IsOldHashFormat(hashedPassword));

            var startTime = DateTime.UtcNow;

            try
            {
                // First try BCrypt verification (new method)
                var bcryptStart = DateTime.UtcNow;
                var bcryptResult = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
                var bcryptEnd = DateTime.UtcNow;
                var bcryptDuration = bcryptEnd - bcryptStart;

                _logger.LogDebug("[USER-SERVICE] BCrypt verification completed in {Duration}ms. Result: {Result}",
                    bcryptDuration.TotalMilliseconds, bcryptResult);

                if (bcryptResult)
                {
                    var totalDuration = DateTime.UtcNow - startTime;
                    _logger.LogInformation("[USER-SERVICE] Password verification successful via BCrypt in {TotalDuration}ms",
                        totalDuration.TotalMilliseconds);
                    return true;
                }

                // Fallback to old GetHashCode method for backward compatibility
                // This allows existing users to log in and their passwords will be re-hashed
                var oldHashStart = DateTime.UtcNow;
                var oldHash = plainPassword.GetHashCode().ToString();
                var oldHashResult = hashedPassword == oldHash;
                var oldHashEnd = DateTime.UtcNow;
                var oldHashDuration = oldHashEnd - oldHashStart;

                _logger.LogDebug("[USER-SERVICE] Old hash verification completed in {Duration}ms. Result: {Result}",
                    oldHashDuration.TotalMilliseconds, oldHashResult);

                if (oldHashResult)
                {
                    var totalDuration = DateTime.UtcNow - startTime;
                    _logger.LogInformation("[USER-SERVICE] Password verification successful via old hash method in {TotalDuration}ms",
                        totalDuration.TotalMilliseconds);
                    return true;
                }

                var totalDurationFail = DateTime.UtcNow - startTime;
                _logger.LogWarning("[USER-SERVICE] Password verification failed for both methods in {TotalDuration}ms",
                    totalDurationFail.TotalMilliseconds);

                return false;
            }
            catch (Exception ex)
            {
                var totalDuration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "[USER-SERVICE] Password verification failed with exception after {TotalDuration}ms. Error: {ErrorMessage}",
                    totalDuration.TotalMilliseconds, ex.Message);
                throw;
            }
        }

        public bool IsOldHashFormat(string hashedPassword)
        {
            // Check if the hash looks like an old GetHashCode format (just numbers)
            var result = int.TryParse(hashedPassword, out _);
            _logger.LogDebug("[USER-SERVICE] Hash format check: {IsOldFormat} for hash starting with: {HashPrefix}",
                result, hashedPassword?.Substring(0, Math.Min(10, hashedPassword?.Length ?? 0)) ?? "null");
            return result;
        }
    }
}