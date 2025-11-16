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
    public class PersonaActivityAssociationRepository : IPersonaActivityAssociationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PersonaActivityAssociationRepository> _logger;

        public PersonaActivityAssociationRepository(ApplicationDbContext context, ILogger<PersonaActivityAssociationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PersonaActivityAssociation> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Starting GetByIdAsync for association ID: {AssociationId}", id);

            var startTime = DateTime.UtcNow;

            try
            {
                var association = await _context.PersonaActivityAssociations
                    .Include(paa => paa.Persona)
                    .Include(paa => paa.Activity)
                    .FirstOrDefaultAsync(paa => paa.Id == id)
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-ACTIVITY-ASSOC-REPO] GetByIdAsync completed in {Duration}ms. Association found: {AssociationFound} for ID: {AssociationId}",
                    duration.TotalMilliseconds, association != null, id);

                return association;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-ACTIVITY-ASSOC-REPO] GetByIdAsync failed after {Duration}ms for association ID: {AssociationId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, id, ex.Message);

                throw;
            }
        }

        public async Task<IEnumerable<PersonaActivityAssociation>> GetByPersonaIdAsync(Guid personaId)
        {
            _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Starting GetByPersonaIdAsync for persona ID: {PersonaId}", personaId);

            var startTime = DateTime.UtcNow;

            try
            {
                var associations = await _context.PersonaActivityAssociations
                    .Include(paa => paa.Persona)
                    .Include(paa => paa.Activity)
                    .Where(paa => paa.PersonaId == personaId)
                    .OrderByDescending(paa => paa.CreatedAt)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-ACTIVITY-ASSOC-REPO] GetByPersonaIdAsync completed in {Duration}ms. Found {Count} associations for persona ID: {PersonaId}",
                    duration.TotalMilliseconds, associations.Count, personaId);

                return associations;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-ACTIVITY-ASSOC-REPO] GetByPersonaIdAsync failed after {Duration}ms for persona ID: {PersonaId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, personaId, ex.Message);

                throw;
            }
        }

        public async Task<IEnumerable<PersonaActivityAssociation>> GetByActivityIdAsync(Guid activityId)
        {
            _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Starting GetByActivityIdAsync for activity ID: {ActivityId}", activityId);

            var startTime = DateTime.UtcNow;

            try
            {
                // Log database state before query
                var totalCount = await _context.PersonaActivityAssociations.CountAsync().ConfigureAwait(false);
                var activityCount = await _context.PersonaActivityAssociations.CountAsync(paa => paa.ActivityId == activityId).ConfigureAwait(false);
                _logger.LogInformation("[PERSONA-ACTIVITY-ASSOC-REPO] Database state - Total associations: {TotalCount}, Associations for activity {ActivityId}: {ActivityCount}",
                    totalCount, activityId, activityCount);

                _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Executing EF query with includes for Persona and Activity");
                _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Query details - ActivityId: {ActivityId}, Include Persona: true, Include Activity: true, OrderBy: CreatedAt DESC",
                    activityId);

                var associations = await _context.PersonaActivityAssociations
                    .Include(paa => paa.Persona)
                    .Include(paa => paa.Activity)
                    .Where(paa => paa.ActivityId == activityId)
                    .OrderByDescending(paa => paa.CreatedAt)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-ACTIVITY-ASSOC-REPO] GetByActivityIdAsync completed in {Duration}ms. Found {Count} associations for activity ID: {ActivityId}",
                    duration.TotalMilliseconds, associations.Count, activityId);

                // Log detailed information about each association
                foreach (var assoc in associations)
                {
                    _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Association details - ID: {AssocId}, PersonaId: {PersonaId}, Persona loaded: {PersonaLoaded}, Activity loaded: {ActivityLoaded}",
                        assoc.Id, assoc.PersonaId, assoc.Persona != null, assoc.Activity != null);

                    if (assoc.Persona != null)
                    {
                        _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Persona details - ID: {PersonaId}, Name: {PersonaName}, UserId: {UserId}",
                            assoc.Persona.Id, assoc.Persona.Name, assoc.Persona.UserId);
                    }
                    else
                    {
                        _logger.LogWarning("[PERSONA-ACTIVITY-ASSOC-REPO] Persona is null for association ID: {AssocId}", assoc.Id);
                    }

                    if (assoc.Activity != null)
                    {
                        _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Activity details - ID: {ActivityId}, Name: {ActivityName}",
                            assoc.Activity.Id, assoc.Activity.Name);
                    }
                    else
                    {
                        _logger.LogWarning("[PERSONA-ACTIVITY-ASSOC-REPO] Activity is null for association ID: {AssocId}", assoc.Id);
                    }
                }

                return associations;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-ACTIVITY-ASSOC-REPO] GetByActivityIdAsync failed after {Duration}ms for activity ID: {ActivityId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, activityId, ex.Message);

                throw;
            }
        }

        public async Task AddAsync(PersonaActivityAssociation association)
        {
            _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Starting AddAsync for association between persona {PersonaId} and activity {ActivityId}",
                association.PersonaId, association.ActivityId);

            var startTime = DateTime.UtcNow;

            try
            {
                await _context.PersonaActivityAssociations.AddAsync(association).ConfigureAwait(false);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-ACTIVITY-ASSOC-REPO] AddAsync completed in {Duration}ms for association ID: {AssociationId}",
                    duration.TotalMilliseconds, association.Id);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-ACTIVITY-ASSOC-REPO] AddAsync failed after {Duration}ms for association between persona {PersonaId} and activity {ActivityId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, association.PersonaId, association.ActivityId, ex.Message);

                throw;
            }
        }

        public async Task UpdateAsync(PersonaActivityAssociation association)
        {
            _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Starting UpdateAsync for association ID: {AssociationId}", association.Id);

            var startTime = DateTime.UtcNow;

            try
            {
                _context.PersonaActivityAssociations.Update(association);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-ACTIVITY-ASSOC-REPO] UpdateAsync completed in {Duration}ms for association ID: {AssociationId}",
                    duration.TotalMilliseconds, association.Id);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-ACTIVITY-ASSOC-REPO] UpdateAsync failed after {Duration}ms for association ID: {AssociationId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, association.Id, ex.Message);

                throw;
            }
        }

        public async Task DeleteAsync(PersonaActivityAssociation association)
        {
            _logger.LogDebug("[PERSONA-ACTIVITY-ASSOC-REPO] Starting DeleteAsync for association ID: {AssociationId}", association.Id);

            var startTime = DateTime.UtcNow;

            try
            {
                _context.PersonaActivityAssociations.Remove(association);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[PERSONA-ACTIVITY-ASSOC-REPO] DeleteAsync completed in {Duration}ms for association ID: {AssociationId}",
                    duration.TotalMilliseconds, association.Id);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[PERSONA-ACTIVITY-ASSOC-REPO] DeleteAsync failed after {Duration}ms for association ID: {AssociationId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, association.Id, ex.Message);

                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid personaId, Guid activityId)
        {
            return await _context.PersonaActivityAssociations
                .AnyAsync(paa => paa.PersonaId == personaId && paa.ActivityId == activityId)
                .ConfigureAwait(false);
        }
    }
}