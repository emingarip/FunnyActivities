using FunnyActivities.Application.AI;
using FunnyActivities.Application.DTOs.Settings;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/settings/ai")]
    [Authorize(Roles = "Admin")]
    public class LlmSettingsController : BaseController
    {
        private readonly ILlmSettingsService _llmSettingsService;
        private readonly IAIService _aiService;
        private readonly IStringLocalizer<LlmSettingsController> _localizer;

        public LlmSettingsController(
            ILlmSettingsService llmSettingsService,
            IAIService aiService,
            ILogger<LlmSettingsController> logger,
            IStringLocalizer<LlmSettingsController> localizer) : base(logger)
        {
            _llmSettingsService = llmSettingsService;
            _aiService = aiService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
        {
            var settings = await _llmSettingsService.GetAsync(cancellationToken);
            return this.ApiSuccess(settings, _localizer["LlmSettingsRetrieved"]);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateLlmSettingsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var updated = await _llmSettingsService.UpdateAsync(request, CurrentUserId, cancellationToken);
                return this.ApiSuccess(updated, _localizer["LlmSettingsUpdated"]);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid AI settings payload");
                return this.ApiError(_localizer["LlmSettingsValidationError"], "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating AI settings");
                return this.ApiError(_localizer["LlmSettingsUnexpectedError"], "InternalError", 500);
            }
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetProviderModels([FromQuery] LlmProvider provider = LlmProvider.Ollama, [FromQuery] bool force = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var models = await _aiService.ListAvailableModelsAsync(provider, force, cancellationToken);
                return this.ApiSuccess(new { provider, models }, _localizer["LlmModelsRetrieved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load models for provider {Provider}", provider);
                return this.ApiError(_localizer["LlmModelsError"], "ProviderModelsError", 500);
            }
        }
    }
}
