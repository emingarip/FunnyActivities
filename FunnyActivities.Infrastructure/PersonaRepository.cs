using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Infrastructure
{
    public class PersonaRepository : IPersonaRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PersonaRepository> _logger;

        public PersonaRepository(ApplicationDbContext context, ILogger<PersonaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Persona> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("[PERSONA-REPO] Starting GetByIdAsync for persona ID: {PersonaId}", id);

            var startTime = DateTime.UtcNow;

            try
            {
                var persona = await _context.Personas
                    .Include(p => p.Characteristics)
                    .Include(p => p.Images)
                    .Include(p => p.ActivityAssociations)
                    .FirstOrDefaultAsync(p => p.Id == id)
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-REPO] GetByIdAsync completed in {Duration}ms. Persona found: {PersonaFound} for ID: {PersonaId}",
                    duration.TotalMilliseconds, persona != null, id);

                return persona;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-REPO] GetByIdAsync failed after {Duration}ms for persona ID: {PersonaId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, id, ex.Message);

                throw;
            }
        }

        public async Task<IEnumerable<Persona>> GetByUserIdAsync(Guid userId)
        {
            _logger.LogDebug("[PERSONA-REPO] Starting GetByUserIdAsync for user ID: {UserId}", userId);

            var startTime = DateTime.UtcNow;

            try
            {
                var personas = await _context.Personas
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Characteristics)
                    .Include(p => p.Images)
                    .Include(p => p.ActivityAssociations)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-REPO] GetByUserIdAsync completed in {Duration}ms. Found {Count} personas for user ID: {UserId}",
                    duration.TotalMilliseconds, personas.Count, userId);

                return personas;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-REPO] GetByUserIdAsync failed after {Duration}ms for user ID: {UserId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, userId, ex.Message);

                throw;
            }
        }

        public async Task AddAsync(Persona persona)
        {
            _logger.LogDebug("[PERSONA-REPO] Starting AddAsync for persona: {PersonaName}", persona.Name);

            var startTime = DateTime.UtcNow;

            try
            {
                await _context.Personas.AddAsync(persona).ConfigureAwait(false);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-REPO] AddAsync completed in {Duration}ms for persona ID: {PersonaId}",
                    duration.TotalMilliseconds, persona.Id);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-REPO] AddAsync failed after {Duration}ms for persona: {PersonaName}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, persona.Name, ex.Message);

                throw;
            }
        }

        public async Task UpdateAsync(Persona persona)
        {
            _logger.LogDebug("[PERSONA-REPO] Starting UpdateAsync for persona ID: {PersonaId}", persona.Id);
            _logger.LogDebug("[PERSONA-REPO] Persona demographic fields before save: Age={Age}, Gender={Gender}, Nationality={Nationality}, Biography={Biography}",
                persona.Age, persona.Gender, persona.Nationality, persona.Biography);

            var startTime = DateTime.UtcNow;

            try
            {
                _context.Personas.Update(persona);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-REPO] UpdateAsync completed in {Duration}ms for persona ID: {PersonaId}",
                    duration.TotalMilliseconds, persona.Id);
                _logger.LogDebug("[PERSONA-REPO] Persona demographic fields after save: Age={Age}, Gender={Gender}, Nationality={Nationality}, Biography={Biography}",
                    persona.Age, persona.Gender, persona.Nationality, persona.Biography);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-REPO] UpdateAsync failed after {Duration}ms for persona ID: {PersonaId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, persona.Id, ex.Message);

                throw;
            }
        }

        public async Task DeleteAsync(Persona persona)
        {
            _logger.LogDebug("[PERSONA-REPO] Starting DeleteAsync for persona ID: {PersonaId}", persona.Id);

            var startTime = DateTime.UtcNow;

            try
            {
                _context.Personas.Remove(persona);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-REPO] DeleteAsync completed in {Duration}ms for persona ID: {PersonaId}",
                    duration.TotalMilliseconds, persona.Id);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-REPO] DeleteAsync failed after {Duration}ms for persona ID: {PersonaId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, persona.Id, ex.Message);

                throw;
            }
        }

        public async Task<bool> ExistsByNameAndUserIdAsync(string name, Guid userId)
        {
            return await _context.Personas.AnyAsync(p => p.Name == name && p.UserId == userId).ConfigureAwait(false);
        }

        public async Task<(IEnumerable<Persona> Personas, int TotalCount)> GetByUserIdPagedAsync(Guid userId, int page, int pageSize, string sortBy, string sortOrder)
        {
            _logger.LogDebug("[PERSONA-REPO] Starting GetByUserIdPagedAsync for user ID: {UserId}, page: {Page}, pageSize: {PageSize}", userId, page, pageSize);

            var startTime = DateTime.UtcNow;

            try
            {
                var query = _context.Personas.Where(p => p.UserId == userId);

                var totalCount = await query.CountAsync().ConfigureAwait(false);

                query = sortBy.ToLower() switch
                {
                    "name" => sortOrder.ToLower() == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    "createdat" => sortOrder.ToLower() == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };

                var personas = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(p => p.Characteristics)
                    .Include(p => p.Images)
                    .Include(p => p.ActivityAssociations)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-REPO] GetByUserIdPagedAsync completed in {Duration}ms. Found {Count} personas for user ID: {UserId}",
                    duration.TotalMilliseconds, personas.Count, userId);

                return (personas, totalCount);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-REPO] GetByUserIdPagedAsync failed after {Duration}ms for user ID: {UserId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, userId, ex.Message);

                throw;
            }
        }
    }
}
