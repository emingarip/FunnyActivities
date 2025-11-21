using System;
using System.Threading.Tasks;
using FunnyActivities.Application.AI;
using FunnyActivities.Application.Commands.ContentGeneration;
using FunnyActivities.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/content")]
    [Authorize]
    public class ContentGenerationController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<ContentGenerationController> _localizer;
        private readonly IAIService _aiService;

        public ContentGenerationController(
            IMediator mediator,
            ILogger<ContentGenerationController> logger,
            IStringLocalizer<ContentGenerationController> localizer,
            IAIService aiService)
            : base(logger)
        {
            _mediator = mediator;
            _localizer = localizer;
            _aiService = aiService;
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
                Model = request.Model,
                Provider = request.Provider,
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                SystemPrompt = request.SystemPrompt
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

        [HttpGet("models")]
        public async Task<IActionResult> GetModels([FromQuery] LlmProvider provider = LlmProvider.Ollama, [FromQuery] bool force = false)
        {
            var models = await _aiService.ListAvailableModelsAsync(provider, force);
            return this.ApiSuccess(new { provider, models }, _localizer["ContentGenerated"]);
        }
    }

    public class GenerateContentRequest
    {
        public Guid PersonaId { get; set; }
        public Guid ActivityId { get; set; }
        public string? CustomPrompt { get; set; }
        public string? Model { get; set; }
        public LlmProvider Provider { get; set; } = LlmProvider.Ollama;
        public float? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public string? SystemPrompt { get; set; }
    }
}
