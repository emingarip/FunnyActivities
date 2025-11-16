using MediatR;
using FunnyActivities.Application.Queries.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class GetActivityPersonaAssociationsQueryHandler : IRequestHandler<GetActivityPersonaAssociationsQuery, List<PersonaActivityAssociationDto>>
    {
        private readonly IPersonaActivityAssociationRepository _personaActivityAssociationRepository;
        private readonly ILogger<GetActivityPersonaAssociationsQueryHandler> _logger;

        public GetActivityPersonaAssociationsQueryHandler(
            IPersonaActivityAssociationRepository personaActivityAssociationRepository,
            ILogger<GetActivityPersonaAssociationsQueryHandler> logger)
        {
            _personaActivityAssociationRepository = personaActivityAssociationRepository;
            _logger = logger;
        }

        public async Task<List<PersonaActivityAssociationDto>> Handle(GetActivityPersonaAssociationsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[GET-ACTIVITY-PERSONA-ASSOC-HANDLER] Starting Handle for activity ID: {ActivityId}", request.ActivityId);
            _logger.LogDebug("[GET-ACTIVITY-PERSONA-ASSOC-HANDLER] ActivityId validation - IsEmpty: {IsEmpty}, IsValidGuid: {IsValidGuid}",
                request.ActivityId == Guid.Empty, Guid.TryParse(request.ActivityId.ToString(), out _));

            var startTime = DateTime.UtcNow;

            try
            {
                // Get all associations for the specified activity ID
                _logger.LogDebug("[GET-ACTIVITY-PERSONA-ASSOC-HANDLER] Calling repository GetByActivityIdAsync for activity ID: {ActivityId}", request.ActivityId);
                var associations = await _personaActivityAssociationRepository.GetByActivityIdAsync(request.ActivityId);

                _logger.LogInformation("[GET-ACTIVITY-PERSONA-ASSOC-HANDLER] Retrieved {Count} associations from repository for activity ID: {ActivityId}",
                    associations?.Count() ?? 0, request.ActivityId);

                // Log details of each association
                if (associations != null)
                {
                    foreach (var assoc in associations)
                    {
                        _logger.LogDebug("[GET-ACTIVITY-PERSONA-ASSOC-HANDLER] Association found - ID: {AssocId}, PersonaId: {PersonaId}, Persona: {PersonaExists}, Activity: {ActivityExists}",
                            assoc.Id, assoc.PersonaId, assoc.Persona != null, assoc.Activity != null);
                    }
                }

                // Map to DTOs with persona data included
                _logger.LogDebug("[GET-ACTIVITY-PERSONA-ASSOC-HANDLER] Starting mapping to DTOs for {Count} associations", associations?.Count() ?? 0);
                var associationDtos = associations.Select(a => new PersonaActivityAssociationDto
                {
                    Id = a.Id,
                    PersonaId = a.PersonaId,
                    Persona = a.Persona != null ? new PersonaDto
                    {
                        Id = a.Persona.Id,
                        UserId = a.Persona.UserId,
                        Name = a.Persona.Name,
                        Description = a.Persona.Description,
                        AvatarImageUrl = a.Persona.AvatarImageUrl,
                        Age = a.Persona.Age,
                        Gender = a.Persona.Gender,
                        Nationality = a.Persona.Nationality,
                        Biography = a.Persona.Biography,
                        Characteristics = a.Persona.Characteristics.Select(c => new PersonaCharacteristicDto
                        {
                            Id = c.Id,
                            PersonaId = c.PersonaId,
                            Name = c.Name,
                            Value = c.Value,
                            Type = c.Type,
                            Order = c.Order,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt
                        }).ToList(),
                        ActivityAssociations = new List<PersonaActivityAssociationDto>(), // Avoid circular reference
                        CreatedAt = a.Persona.CreatedAt,
                        UpdatedAt = a.Persona.UpdatedAt
                    } : null,
                    ActivityId = a.ActivityId,
                    ActivityName = a.Activity?.Name ?? string.Empty,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList();

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[GET-ACTIVITY-PERSONA-ASSOC-HANDLER] Handle completed in {Duration}ms. Returning {Count} DTOs for activity ID: {ActivityId}",
                    duration.TotalMilliseconds, associationDtos.Count, request.ActivityId);

                return associationDtos;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[GET-ACTIVITY-PERSONA-ASSOC-HANDLER] Handle failed after {Duration}ms for activity ID: {ActivityId}. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, request.ActivityId, ex.Message);

                throw;
            }
        }
    }
}