using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FunnyActivities.CrossCuttingConcerns.Caching;

namespace FunnyActivities.Infrastructure
{
    public class ActivityCategoryRepository : IActivityCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cache;

        public ActivityCategoryRepository(ApplicationDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ActivityCategory?> GetByIdAsync(Guid id)
        {
            return await _context.ActivityCategories.FirstOrDefaultAsync(ac => ac.Id == id);
        }

        public async Task<List<ActivityCategory>> GetAllAsync()
        {
            const string cacheKey = "activity_categories";
            var cachedCategories = await _cache.GetAsync<List<ActivityCategory>>(cacheKey);

            if (cachedCategories != null)
            {
                return cachedCategories;
            }

            var categories = await _context.ActivityCategories
                .OrderBy(ac => ac.Name)
                .ToListAsync();

            // Cache for 30 minutes
            await _cache.SetAsync(cacheKey, categories, TimeSpan.FromMinutes(30));
            return categories;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.ActivityCategories
                .AnyAsync(ac => ac.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId)
        {
            return await _context.ActivityCategories
                .AnyAsync(ac => ac.Name.ToLower() == name.ToLower() && ac.Id != excludeId);
        }

        public async Task AddAsync(ActivityCategory category)
        {
            await _context.ActivityCategories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ActivityCategory category)
        {
            _context.ActivityCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ActivityCategory category)
        {
            _context.ActivityCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}