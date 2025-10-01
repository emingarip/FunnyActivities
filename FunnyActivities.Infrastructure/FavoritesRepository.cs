using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Infrastructure
{
    public class FavoritesRepository : FunnyActivities.Domain.Interfaces.IFavoritesRepository, FunnyActivities.Application.Interfaces.IFavoritesRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavoritesRepository> _logger;

        public FavoritesRepository(ApplicationDbContext context, ILogger<FavoritesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Favorites> GetByIdAsync(Guid id)
        {
            return await _context.Favorites.FindAsync(id).ConfigureAwait(false);
        }

        public async Task AddAsync(Favorites favorites)
        {
            _logger.LogDebug("[FAVORITES-REPO] Starting AddAsync for user {UserId}, activity {ActivityId}", favorites.UserId, favorites.ActivityId);

            var startTime = DateTime.UtcNow;

            try
            {
                await _context.Favorites.AddAsync(favorites).ConfigureAwait(false);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[FAVORITES-REPO] AddAsync completed in {Duration}ms for user {UserId}, activity {ActivityId}",
                    duration.TotalMilliseconds, favorites.UserId, favorites.ActivityId);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[FAVORITES-REPO] AddAsync failed after {Duration}ms for user {UserId}, activity {ActivityId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, favorites.UserId, favorites.ActivityId, ex.Message);

                throw;
            }
        }

        public async Task UpdateAsync(Favorites favorites)
        {
            _logger.LogDebug("[FAVORITES-REPO] Starting UpdateAsync for favorite {FavoriteId}", favorites.Id);

            var startTime = DateTime.UtcNow;

            try
            {
                _context.Favorites.Update(favorites);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[FAVORITES-REPO] UpdateAsync completed in {Duration}ms for favorite {FavoriteId}",
                    duration.TotalMilliseconds, favorites.Id);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[FAVORITES-REPO] UpdateAsync failed after {Duration}ms for favorite {FavoriteId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, favorites.Id, ex.Message);

                throw;
            }
        }

        public async Task DeleteAsync(Favorites favorites)
        {
            _logger.LogDebug("[FAVORITES-REPO] Starting DeleteAsync for favorite {FavoriteId}", favorites.Id);

            var startTime = DateTime.UtcNow;

            try
            {
                _context.Favorites.Remove(favorites);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[FAVORITES-REPO] DeleteAsync completed in {Duration}ms for favorite {FavoriteId}",
                    duration.TotalMilliseconds, favorites.Id);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[FAVORITES-REPO] DeleteAsync failed after {Duration}ms for favorite {FavoriteId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, favorites.Id, ex.Message);

                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid activityId)
        {
            _logger.LogDebug("[FAVORITES-REPO] Starting ExistsAsync for user {UserId}, activity {ActivityId}", userId, activityId);

            var startTime = DateTime.UtcNow;

            try
            {
                var exists = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ActivityId == activityId).ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[FAVORITES-REPO] ExistsAsync completed in {Duration}ms for user {UserId}, activity {ActivityId}. Result: {Exists}",
                    duration.TotalMilliseconds, userId, activityId, exists);

                return exists;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[FAVORITES-REPO] ExistsAsync failed after {Duration}ms for user {UserId}, activity {ActivityId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, userId, activityId, ex.Message);

                throw;
            }
        }

        public async Task<Favorites> GetByUserAndActivityAsync(Guid userId, Guid activityId)
        {
            _logger.LogDebug("[FAVORITES-REPO] Starting GetByUserAndActivityAsync for user {UserId}, activity {ActivityId}", userId, activityId);

            var startTime = DateTime.UtcNow;

            try
            {
                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ActivityId == activityId)
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[FAVORITES-REPO] GetByUserAndActivityAsync completed in {Duration}ms for user {UserId}, activity {ActivityId}. Found: {Found}",
                    duration.TotalMilliseconds, userId, activityId, favorite != null);

                return favorite;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[FAVORITES-REPO] GetByUserAndActivityAsync failed after {Duration}ms for user {UserId}, activity {ActivityId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, userId, activityId, ex.Message);

                throw;
            }
        }

        public async Task<(IEnumerable<Favorites> Favorites, int TotalCount)> GetUserFavoritesAsync(Guid userId, int page, int pageSize)
        {
            _logger.LogDebug("[FAVORITES-REPO] Starting GetUserFavoritesAsync for user {UserId}, page {Page}, pageSize {PageSize}", userId, page, pageSize);

            var startTime = DateTime.UtcNow;

            try
            {
                var query = _context.Favorites
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt);

                var totalCount = await query.CountAsync().ConfigureAwait(false);
                var favorites = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[FAVORITES-REPO] GetUserFavoritesAsync completed in {Duration}ms for user {UserId}. Retrieved {Count} favorites out of {TotalCount}",
                    duration.TotalMilliseconds, userId, favorites.Count, totalCount);

                return (favorites, totalCount);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[FAVORITES-REPO] GetUserFavoritesAsync failed after {Duration}ms for user {UserId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, userId, ex.Message);

                throw;
            }
        }

        public async Task<IEnumerable<Favorites>> GetUserFavoritesAsync(Guid userId)
        {
            _logger.LogDebug("[FAVORITES-REPO] Starting GetUserFavoritesAsync for user {UserId}", userId);

            var startTime = DateTime.UtcNow;

            try
            {
                var favorites = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[FAVORITES-REPO] GetUserFavoritesAsync completed in {Duration}ms for user {UserId}. Retrieved {Count} favorites",
                    duration.TotalMilliseconds, userId, favorites.Count);

                return favorites;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[FAVORITES-REPO] GetUserFavoritesAsync failed after {Duration}ms for user {UserId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, userId, ex.Message);

                throw;
            }
        }

        public async Task<int> GetUserFavoritesCountAsync(Guid userId)
        {
            _logger.LogDebug("[FAVORITES-REPO] Starting GetUserFavoritesCountAsync for user {UserId}", userId);

            var startTime = DateTime.UtcNow;

            try
            {
                var count = await _context.Favorites.CountAsync(f => f.UserId == userId).ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[FAVORITES-REPO] GetUserFavoritesCountAsync completed in {Duration}ms for user {UserId}. Count: {Count}",
                    duration.TotalMilliseconds, userId, count);

                return count;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[FAVORITES-REPO] GetUserFavoritesCountAsync failed after {Duration}ms for user {UserId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, userId, ex.Message);

                throw;
            }
        }
    }
}