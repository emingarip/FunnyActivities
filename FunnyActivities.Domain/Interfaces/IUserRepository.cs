using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task AddAsync(User user);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User> GetByEmailAsync(string email);
        Task UpdateAsync(User user);
        Task<User> GetByResetTokenAsync(string token);
        Task<(IEnumerable<User> Users, int TotalCount)> SearchAsync(string searchTerm, int page, int pageSize, string sortBy, string sortOrder);
        Task<int> GetOnlineUsersCountAsync(TimeSpan onlineThreshold);
        Task<List<UserGrowthDataPoint>> GetUserGrowthDataAsync(string period, int days);
    }
}