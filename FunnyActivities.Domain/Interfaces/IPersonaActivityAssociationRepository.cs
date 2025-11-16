using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IPersonaActivityAssociationRepository
    {
        Task<PersonaActivityAssociation> GetByIdAsync(Guid id);
        Task<IEnumerable<PersonaActivityAssociation>> GetByPersonaIdAsync(Guid personaId);
        Task<IEnumerable<PersonaActivityAssociation>> GetByActivityIdAsync(Guid activityId);
        Task AddAsync(PersonaActivityAssociation association);
        Task UpdateAsync(PersonaActivityAssociation association);
        Task DeleteAsync(PersonaActivityAssociation association);
        Task<bool> ExistsAsync(Guid personaId, Guid activityId);
    }
}