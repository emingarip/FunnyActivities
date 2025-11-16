using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.ContentGeneration;
using Microsoft.AspNetCore.Authorization;
using FunnyActivities.WebAPI.Controllers.Base;
using System.Threading.Tasks;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/content")]
    [Authorize]
    public class ContentGenerationController : BaseController
    {
        private readonly IMediator _mediator;

        public ContentGenerationController(IMediator mediator, ILogger<ContentGenerationController> logger)
            : base(logger)
        {
            _mediator = mediator;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateContent([FromBody] GenerateContentRequest request)
        {
            var command = new GenerateContentCommand
            {
                UserId = CurrentUserId,
                PersonaId = request.PersonaId,
                ActivityId = request.ActivityId,
                CustomPrompt = request.CustomPrompt,
                Model = request.Model ?? "llama2"
            };

            var content = await _mediator.Send(command);
            return Ok(new { content });
        }
    }

    public class GenerateContentRequest
    {
        public Guid PersonaId { get; set; }
        public Guid ActivityId { get; set; }
        public string? CustomPrompt { get; set; }
        public string? Model { get; set; }
    }
}