using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IFavoritesRepository
    {
        Task<Favorites> GetByIdAsync(Guid id);
        Task AddAsync(Favorites favorites);
        Task UpdateAsync(Favorites favorites);
        Task DeleteAsync(Favorites favorites);
        Task<bool> ExistsAsync(Guid userId, Guid activityId);
        Task<Favorites> GetByUserAndActivityAsync(Guid userId, Guid activityId);
        Task<(IEnumerable<Favorites> Favorites, int TotalCount)> GetUserFavoritesAsync(Guid userId, int page, int pageSize);
        Task<IEnumerable<Favorites>> GetUserFavoritesAsync(Guid userId);
        Task<int> GetUserFavoritesCountAsync(Guid userId);
    }
}