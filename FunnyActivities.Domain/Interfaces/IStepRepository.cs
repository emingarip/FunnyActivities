using System;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IStepRepository
    {
        Task<Step> GetByIdAsync(Guid id);
        Task AddAsync(Step step);
        Task UpdateAsync(Step step);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Step>> GetByActivityIdAsync(Guid activityId);
    }
}