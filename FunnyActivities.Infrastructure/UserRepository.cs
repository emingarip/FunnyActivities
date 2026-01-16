using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FunnyActivities.Infrastructure
{
    public class UserRepository : FunnyActivities.Domain.Interfaces.IUserRepository, FunnyActivities.Application.Interfaces.IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository>? logger = null)
        {
            _context = context;
            _logger = logger ?? NullLogger<UserRepository>.Instance;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id).ConfigureAwait(false);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email).ConfigureAwait(false);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            _logger.LogDebug("[USER-REPO] Starting GetByEmailAsync for email: {Email}", MaskEmail(email));

            var startTime = DateTime.UtcNow;

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email).ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[USER-REPO] GetByEmailAsync completed in {Duration}ms. User found: {UserFound} for email: {Email}",
                    duration.TotalMilliseconds, user != null, MaskEmail(email));

                if (user != null)
                {
                    _logger.LogDebug("[USER-REPO] Found user with ID: {UserId}, Role: {Role}", user.Id, user.Role);
                }

                return user;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[USER-REPO] GetByEmailAsync failed after {Duration}ms for email: {Email}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, MaskEmail(email), ex.Message);

                throw;
            }
        }

        public async Task UpdateAsync(User user)
        {
            _logger.LogDebug("[USER-REPO] Starting UpdateAsync for user ID: {UserId}", user.Id);

            var startTime = DateTime.UtcNow;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[USER-REPO] UpdateAsync completed in {Duration}ms for user ID: {UserId}",
                    duration.TotalMilliseconds, user.Id);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[USER-REPO] UpdateAsync failed after {Duration}ms for user ID: {UserId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, user.Id, ex.Message);

                throw;
            }
        }

        public async Task<User> GetByResetTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow).ConfigureAwait(false);
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> SearchAsync(string searchTerm, int page, int pageSize, string sortBy, string sortOrder)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Email.Contains(searchTerm) || u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync().ConfigureAwait(false);

            query = sortBy.ToLower() switch
            {
                "email" => sortOrder.ToLower() == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "firstname" => sortOrder.ToLower() == "asc" ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
                "lastname" => sortOrder.ToLower() == "asc" ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
                "createdat" => sortOrder.ToLower() == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return (users, totalCount);
        }

        public async Task<int> GetTotalCountAsync()
        {
            _logger.LogDebug("[USER-REPO] Starting GetTotalCountAsync");

            var startTime = DateTime.UtcNow;

            try
            {
                var count = await _context.Users.CountAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[USER-REPO] GetTotalCountAsync completed in {Duration}ms. Total users: {Count}",
                    duration.TotalMilliseconds, count);

                return count;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[USER-REPO] GetTotalCountAsync failed after {Duration}ms. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, ex.Message);

                throw;
            }
        }

        public async Task<int> GetOnlineUsersCountAsync(TimeSpan onlineThreshold)
        {
            _logger.LogDebug("[USER-REPO] Starting GetOnlineUsersCountAsync with threshold: {Threshold} minutes", onlineThreshold.TotalMinutes);

            var startTime = DateTime.UtcNow;

            try
            {
                var thresholdDate = DateTime.UtcNow - onlineThreshold;
                var count = await _context.Users.CountAsync(u => u.LastLoginDate >= thresholdDate).ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[USER-REPO] GetOnlineUsersCountAsync completed in {Duration}ms. Online users: {Count}",
                    duration.TotalMilliseconds, count);

                return count;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[USER-REPO] GetOnlineUsersCountAsync failed after {Duration}ms. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, ex.Message);

                throw;
            }
        }

        public async Task<List<UserGrowthDataPoint>> GetUserGrowthDataAsync(string period, int days)
        {
            _logger.LogDebug("[USER-REPO] Starting GetUserGrowthDataAsync with period: {Period}, days: {Days}", period, days);

            var startTime = DateTime.UtcNow;

            try
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days);

                var users = await _context.Users
                    .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                    .Select(u => new { u.CreatedAt.Date })
                    .ToListAsync()
                    .ConfigureAwait(false);

                var dataPoints = new List<UserGrowthDataPoint>();
                var currentDate = startDate;
                var cumulativeCount = await _context.Users.CountAsync(u => u.CreatedAt < startDate).ConfigureAwait(false);

                while (currentDate <= endDate)
                {
                    var dateStr = currentDate.ToString("yyyy-MM-dd");
                    var newUsersOnDate = users.Count(u => u.Date == currentDate);
                    cumulativeCount += newUsersOnDate;

                    dataPoints.Add(new UserGrowthDataPoint
                    {
                        Date = dateStr,
                        Count = cumulativeCount
                    });

                    // Increment based on period
                    currentDate = period.ToLower() switch
                    {
                        "weekly" => currentDate.AddDays(1),
                        "monthly" => currentDate.AddDays(1),
                        "quarterly" => currentDate.AddDays(1),
                        _ => currentDate.AddDays(1)
                    };
                }

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[USER-REPO] GetUserGrowthDataAsync completed in {Duration}ms. Data points: {Count}",
                    duration.TotalMilliseconds, dataPoints.Count);

                return dataPoints;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[USER-REPO] GetUserGrowthDataAsync failed after {Duration}ms. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, ex.Message);

                throw;
            }
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return "***";
            var atIndex = email.IndexOf('@');
            if (atIndex > 3)
                return email.Substring(0, 3) + "***" + email.Substring(atIndex);
            return "***" + email.Substring(atIndex);
        }
    }
}
