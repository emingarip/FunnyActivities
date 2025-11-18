using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.ContentGeneration;
using Microsoft.AspNetCore.Authorization;
using FunnyActivities.WebAPI.Controllers.Base;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/content")]
    [Authorize]
    public class ContentGenerationController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<ContentGenerationController> _localizer;

        public ContentGenerationController(IMediator mediator, ILogger<ContentGenerationController> logger, IStringLocalizer<ContentGenerationController> localizer)
            : base(logger)
        {
            _mediator = mediator;
            _localizer = localizer;
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

            try
            {
                var content = await _mediator.Send(command);
                return this.ApiSuccess(new { content }, _localizer["ContentGenerated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Content generation failed for user {UserId}", CurrentUserId);
                return this.ApiError(_localizer["ContentGenerationUnexpected"], "InternalError", 500);
            }
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
