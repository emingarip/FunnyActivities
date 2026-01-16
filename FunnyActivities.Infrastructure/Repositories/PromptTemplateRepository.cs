using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FunnyActivities.Infrastructure.Repositories
{
    public class PromptTemplateRepository : IPromptTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public PromptTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<PromptTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PromptTemplates
                .AsNoTracking()
                .OrderBy(t => t.Key)
                .ThenBy(t => t.Locale)
                .ToListAsync(cancellationToken);
        }

        public async Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.PromptTemplates
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<PromptTemplate?> GetByKeyAsync(string key, string locale, CancellationToken cancellationToken = default)
        {
            return await _context.PromptTemplates
                .FirstOrDefaultAsync(t => t.Key == key && t.Locale == locale, cancellationToken);
        }

        public async Task<bool> ExistsWithKeyAsync(string key, string locale, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            return await _context.PromptTemplates.AnyAsync(t =>
                t.Key == key &&
                t.Locale == locale &&
                (!excludeId.HasValue || t.Id != excludeId), cancellationToken);
        }

        public async Task AddAsync(PromptTemplate template, CancellationToken cancellationToken = default)
        {
            _context.PromptTemplates.Add(template);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<PromptTemplate> templates, CancellationToken cancellationToken = default)
        {
            _context.PromptTemplates.AddRange(templates);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default)
        {
            _context.PromptTemplates.Update(template);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(PromptTemplate template, CancellationToken cancellationToken = default)
        {
            _context.PromptTemplates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
