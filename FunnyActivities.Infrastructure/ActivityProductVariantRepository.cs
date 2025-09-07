using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FunnyActivities.Infrastructure
{
    public class ActivityProductVariantRepository : IActivityProductVariantRepository
    {
        private readonly ApplicationDbContext _context;

        public ActivityProductVariantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActivityProductVariant?> GetByIdAsync(Guid id)
        {
            return await _context.ActivityProductVariants
                .Include(apv => apv.ProductVariant)
                .Include(apv => apv.UnitOfMeasure)
                .FirstOrDefaultAsync(apv => apv.Id == id);
        }

        public async Task<List<ActivityProductVariant>> GetByActivityIdAsync(Guid activityId)
        {
            return await _context.ActivityProductVariants
                .Include(apv => apv.ProductVariant)
                    .ThenInclude(pv => pv.BaseProduct)
                .Include(apv => apv.UnitOfMeasure)
                .Where(apv => apv.ActivityId == activityId)
                .OrderBy(apv => apv.ProductVariant.BaseProduct.Name)
                .ThenBy(apv => apv.ProductVariant.Name)
                .ToListAsync();
        }

        public async Task<List<ActivityProductVariant>> GetByProductVariantIdAsync(Guid productVariantId)
        {
            return await _context.ActivityProductVariants
                .Include(apv => apv.Activity)
                .Include(apv => apv.UnitOfMeasure)
                .Where(apv => apv.ProductVariantId == productVariantId)
                .ToListAsync();
        }

        public async Task AddAsync(ActivityProductVariant activityProductVariant)
        {
            await _context.ActivityProductVariants.AddAsync(activityProductVariant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ActivityProductVariant activityProductVariant)
        {
            _context.ActivityProductVariants.Update(activityProductVariant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ActivityProductVariant activityProductVariant)
        {
            _context.ActivityProductVariants.Remove(activityProductVariant);
            await _context.SaveChangesAsync();
        }
    }
}