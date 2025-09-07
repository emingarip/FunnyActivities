using System;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IActivityProductVariantRepository
    {
        Task<ActivityProductVariant> GetByIdAsync(Guid id);
        Task AddAsync(ActivityProductVariant activityProductVariant);
        Task UpdateAsync(ActivityProductVariant activityProductVariant);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<ActivityProductVariant>> GetByActivityIdAsync(Guid activityId);
        Task<IEnumerable<ActivityProductVariant>> GetByProductVariantIdAsync(Guid productVariantId);
    }
}