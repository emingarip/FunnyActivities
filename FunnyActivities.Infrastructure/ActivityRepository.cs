using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FunnyActivities.Infrastructure
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public ActivityRepository(ApplicationDbContext context)
        {
            _context = context;
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
    }
}