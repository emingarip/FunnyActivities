using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IPromptTemplateRepository
    {
        Task<IReadOnlyList<PromptTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PromptTemplate?> GetByKeyAsync(string key, string locale, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithKeyAsync(string key, string locale, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task AddAsync(PromptTemplate template, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<PromptTemplate> templates, CancellationToken cancellationToken = default);
        Task UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default);
        Task DeleteAsync(PromptTemplate template, CancellationToken cancellationToken = default);
    }
}
