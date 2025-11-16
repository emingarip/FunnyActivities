using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Application.Queries.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;
using FunnyActivities.Application.DTOs.Shared;
using Microsoft.AspNetCore.Authorization;
using FunnyActivities.WebAPI.Controllers.Base;
using FunnyActivities.Domain.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/personas")]
    [Authorize]
    public class PersonaController : BaseController
    {
        private readonly IMediator _mediator;

        public PersonaController(IMediator mediator, ILogger<PersonaController> logger)
            : base(logger)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePersona([FromBody] CreatePersonaRequest request)
        {
            var command = new CreatePersonaCommand
            {
                UserId = CurrentUserId,
                Name = request.Name,
                Description = request.Description,
                AvatarImageUrl = request.AvatarImageUrl,
                Characteristics = request.Characteristics
            };

            var persona = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPersona), new { id = persona.Id }, persona);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPersona(Guid id)
        {
            var query = new GetPersonaQuery { Id = id, UserId = CurrentUserId };
            var persona = await _mediator.Send(query);

            return Ok(persona);
        }

        [HttpGet]
        public async Task<IActionResult> GetPersonas([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "CreatedAt", [FromQuery] string sortOrder = "desc")
        {
            var query = new GetPersonasQuery
            {
                UserId = CurrentUserId,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePersona(Guid id, [FromBody] UpdatePersonaRequest request)
        {
            _logger.LogInformation("[PERSONA-CONTROLLER] UpdatePersona called with ID: {Id}, Gender: {Gender} (type: {GenderType})", id, request.Gender, request.Gender?.GetType().Name);

            // Convert string gender to enum
            Gender? genderEnum = null;
            if (!string.IsNullOrEmpty(request.Gender))
            {
                if (Enum.TryParse<Gender>(request.Gender, true, out var parsedGender))
                {
                    genderEnum = parsedGender;
                }
                else
                {
                    _logger.LogWarning("[PERSONA-CONTROLLER] Invalid gender string received: {Gender}", request.Gender);
                    return BadRequest($"Invalid gender value: {request.Gender}");
                }
            }

            var command = new UpdatePersonaCommand
            {
                Id = id,
                UserId = CurrentUserId,
                Name = request.Name,
                Description = request.Description,
                AvatarImageUrl = request.AvatarImageUrl,
                Age = request.Age,
                Gender = genderEnum,
                Nationality = request.Nationality,
                Biography = request.Biography
            };

            var persona = await _mediator.Send(command);
            return Ok(persona);
        }

        [HttpPost("{personaId}/images")]
        public async Task<IActionResult> UploadPersonaImages(Guid personaId, [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("Yüklenecek dosya bulunamadı.");
            }

            var uploadFiles = new List<UploadPersonaImageFile>();

            foreach (var file in files)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                uploadFiles.Add(new UploadPersonaImageFile
                {
                    Data = memoryStream.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType
                });
            }

            var command = new UploadPersonaImagesCommand
            {
                PersonaId = personaId,
                UserId = CurrentUserId,
                Files = uploadFiles
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersona(Guid id)
        {
            var command = new DeletePersonaCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            await _mediator.Send(command);
            return NoContent();
        }

        // Persona Activity Association endpoints
        [HttpPost("{personaId}/activities")]
        public async Task<IActionResult> CreatePersonaActivityAssociation(Guid personaId, [FromBody] CreatePersonaActivityAssociationRequest request)
        {
            var command = new CreatePersonaActivityAssociationCommand
            {
                PersonaId = personaId,
                ActivityId = request.ActivityId
            };

            var association = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPersonaActivityAssociations), new { personaId = personaId }, association);
        }

        [HttpGet("{personaId}/activities")]
        public async Task<IActionResult> GetPersonaActivityAssociations(Guid personaId)
        {
            var query = new GetPersonaActivityAssociationsQuery { PersonaId = personaId };
            var associations = await _mediator.Send(query);
            return Ok(associations);
        }

        [HttpPut("activities/{id}")]
        public async Task<IActionResult> UpdatePersonaActivityAssociation(Guid id)
        {
            // Since there's no preference level to update, this endpoint might not be needed
            // But keeping it for potential future use or to maintain API compatibility
            return Ok();
        }

        [HttpDelete("activities/{id}")]
        public async Task<IActionResult> DeletePersonaActivityAssociation(Guid id)
        {
            var command = new DeletePersonaActivityAssociationCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        // Activity-side persona association endpoints
        [HttpPost("activities/{activityId}/personas")]
        public async Task<IActionResult> CreateActivityPersonaAssociation(Guid activityId, [FromBody] CreateActivityPersonaAssociationRequest request)
        {
            var command = new CreateActivityPersonaAssociationCommand
            {
                ActivityId = activityId,
                PersonaId = request.PersonaId
            };

            var association = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetActivityPersonaAssociations), new { activityId = activityId }, association);
        }

        [HttpGet("activities/{activityId}/personas")]
        public async Task<IActionResult> GetActivityPersonaAssociations(Guid activityId)
        {
            var query = new GetActivityPersonaAssociationsQuery { ActivityId = activityId };
            var associations = await _mediator.Send(query);
            return Ok(associations);
        }

        [HttpPut("activities/{id}/personas")]
        public async Task<IActionResult> UpdateActivityPersonaAssociation(Guid id)
        {
            // Since there's no preference level to update, this endpoint might not be needed
            // But keeping it for potential future use or to maintain API compatibility
            return Ok();
        }

        [HttpDelete("activities/{id}/personas")]
        public async Task<IActionResult> DeleteActivityPersonaAssociation(Guid id)
        {
            var command = new DeleteActivityPersonaAssociationCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }

    public class CreatePersonaRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? AvatarImageUrl { get; set; }
        public List<PersonaCharacteristicDto>? Characteristics { get; set; }
    }

    public class UpdatePersonaRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? AvatarImageUrl { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
    }

    public class CreatePersonaActivityAssociationRequest
    {
        public Guid ActivityId { get; set; }
    }


    public class CreateActivityPersonaAssociationRequest
    {
        public Guid PersonaId { get; set; }
    }

}
