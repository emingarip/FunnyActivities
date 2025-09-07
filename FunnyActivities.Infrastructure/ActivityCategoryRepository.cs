using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FunnyActivities.Infrastructure
{
    public class ActivityCategoryRepository : IActivityCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ActivityCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActivityCategory?> GetByIdAsync(Guid id)
        {
            return await _context.ActivityCategories.FirstOrDefaultAsync(ac => ac.Id == id);
        }

        public async Task<List<ActivityCategory>> GetAllAsync()
        {
            return await _context.ActivityCategories
                .OrderBy(ac => ac.Name)
                .ToListAsync();
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