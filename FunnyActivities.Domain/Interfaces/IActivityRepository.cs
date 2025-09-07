using System;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IActivityRepository
    {
        Task<Activity> GetByIdAsync(Guid id);
        Task AddAsync(Activity activity);
        Task UpdateAsync(Activity activity);
        Task DeleteAsync(Guid id);
        Task<(IEnumerable<Activity> Activities, int TotalCount)> GetByCategoryIdAsync(Guid categoryId, int page, int pageSize);
        Task<IEnumerable<Activity>> GetAllAsync();
    }
}