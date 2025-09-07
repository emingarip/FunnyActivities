using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FunnyActivities.Infrastructure
{
    public class StepRepository : IStepRepository
    {
        private readonly ApplicationDbContext _context;

        public StepRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Step?> GetByIdAsync(Guid id)
        {
            return await _context.Steps.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Step>> GetByActivityIdAsync(Guid activityId)
        {
            return await _context.Steps
                .Where(s => s.ActivityId == activityId)
                .OrderBy(s => s.Order)
                .ToListAsync();
        }

        public async Task AddAsync(Step step)
        {
            await _context.Steps.AddAsync(step);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Step step)
        {
            _context.Steps.Update(step);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Step step)
        {
            _context.Steps.Remove(step);
            await _context.SaveChangesAsync();
        }
    }
}