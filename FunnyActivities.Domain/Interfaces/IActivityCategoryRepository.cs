using System;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IActivityCategoryRepository
    {
        Task<ActivityCategory> GetByIdAsync(Guid id);
        Task AddAsync(ActivityCategory category);
        Task UpdateAsync(ActivityCategory category);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<ActivityCategory>> GetAllAsync();
        Task<bool> ExistsByNameAsync(string name);
    }
}