using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FunnyActivities.CrossCuttingConcerns.Caching;

namespace FunnyActivities.Infrastructure
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cache;

        public ActivityRepository(ApplicationDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        private class CachedActivityResult
        {
            public IEnumerable<Activity> Activities { get; set; }
            public int TotalCount { get; set; }
        }

        public async Task<Activity?> GetByIdAsync(Guid id)
        {
            return await _context.Activities
                .Include(a => a.ActivityCategory)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<(IEnumerable<Activity> Activities, int TotalCount)> GetByCategoryIdAsync(Guid categoryId, int page, int pageSize)
        {
            var query = _context.Activities
                .Include(a => a.ActivityCategory)
                .Where(a => a.ActivityCategoryId == categoryId);

            var totalCount = await query.CountAsync();
            var activities = await query
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (activities, totalCount);
        }

        public async Task<List<Activity>> GetAllAsync()
        {
            return await _context.Activities
                .Include(a => a.ActivityCategory)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Activity activity)
        {
            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Activity activity)
        {
            _context.Activities.Update(activity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Activity activity)
        {
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Activity> Activities, int TotalCount)> GetFilteredAsync(
            string? searchTerm,
            Guid? activityCategoryId,
            bool isPublic,
            string? sortBy,
            string? sortOrder,
            int pageNumber,
            int pageSize)
        {
            // For public activities without search term or category filter, use cache
            if (isPublic && string.IsNullOrWhiteSpace(searchTerm) && !activityCategoryId.HasValue)
            {
                var cacheKey = $"public_activities_{pageNumber}_{pageSize}_{sortBy ?? "name"}_{sortOrder ?? "asc"}";
                var cachedResult = await _cache.GetAsync<CachedActivityResult>(cacheKey);

                if (cachedResult != null)
                {
                    return (cachedResult.Activities, cachedResult.TotalCount);
                }

                var result = await GetFilteredActivitiesFromDatabase(searchTerm, activityCategoryId, isPublic, sortBy, sortOrder, pageNumber, pageSize);

                // Cache for 15 minutes
                var cacheData = new CachedActivityResult { Activities = result.Activities, TotalCount = result.TotalCount };
                await _cache.SetAsync(cacheKey, cacheData, TimeSpan.FromMinutes(15));
                return result;
            }

            // For other queries, don't cache (too many variations)
            return await GetFilteredActivitiesFromDatabase(searchTerm, activityCategoryId, isPublic, sortBy, sortOrder, pageNumber, pageSize);
        }

        private async Task<(IEnumerable<Activity> Activities, int TotalCount)> GetFilteredActivitiesFromDatabase(
            string? searchTerm,
            Guid? activityCategoryId,
            bool isPublic,
            string? sortBy,
            string? sortOrder,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Activities
                .Include(a => a.ActivityCategory)
                .AsQueryable();

            // Apply search term filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a => a.Name.Contains(searchTerm) ||
                                          (a.Description != null && a.Description.Contains(searchTerm)));
            }

            // Apply category filtering
            if (activityCategoryId.HasValue)
            {
                query = query.Where(a => a.ActivityCategoryId == activityCategoryId.Value);
            }

            // Apply public filtering
            if (isPublic)
            {
                query = query.Where(a => a.IsPublic);
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(a => a.Name)
                    : query.OrderBy(a => a.Name),
                "createdat" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(a => a.CreatedAt)
                    : query.OrderBy(a => a.CreatedAt),
                "updatedat" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(a => a.UpdatedAt)
                    : query.OrderBy(a => a.UpdatedAt),
                _ => query.OrderBy(a => a.Name)
            };

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var activities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (activities, totalCount);
        }
    }
}