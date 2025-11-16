using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IPersonaRepository
    {
        Task<Persona> GetByIdAsync(Guid id);
        Task<IEnumerable<Persona>> GetByUserIdAsync(Guid userId);
        Task AddAsync(Persona persona);
        Task UpdateAsync(Persona persona);
        Task DeleteAsync(Persona persona);
        Task<bool> ExistsByNameAndUserIdAsync(string name, Guid userId);
        Task<(IEnumerable<Persona> Personas, int TotalCount)> GetByUserIdPagedAsync(Guid userId, int page, int pageSize, string sortBy, string sortOrder);
    }
}