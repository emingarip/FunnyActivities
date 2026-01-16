using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.DTOs.PromptTemplates;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/prompts")]
    [Authorize(Roles = "Admin")]
    public class PromptTemplatesController : BaseController
    {
        private readonly IPromptTemplateService _promptTemplateService;

        public PromptTemplatesController(
            IPromptTemplateService promptTemplateService,
            ILogger<PromptTemplatesController> logger) : base(logger)
        {
            _promptTemplateService = promptTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? locale = null, [FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var templates = await _promptTemplateService.GetTemplatesAsync(locale, includeInactive, cancellationToken);
            return this.ApiSuccess(templates, "Prompt templates retrieved");
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default)
        {
            var template = await _promptTemplateService.GetTemplateAsync(id, cancellationToken);
            if (template == null)
            {
                return this.ApiError("Prompt template not found", "NotFound", 404);
            }

            return this.ApiSuccess(template, "Prompt template retrieved");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePromptTemplateRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var created = await _promptTemplateService.CreateAsync(request, CurrentUserId, cancellationToken);
                return this.ApiCreated(nameof(Get), new { id = created.Id }, created, "Prompt template created");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate prompt template key");
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromptTemplateRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var updated = await _promptTemplateService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
                return this.ApiSuccess(updated, "Prompt template updated");
            }
            catch (KeyNotFoundException)
            {
                return this.ApiError("Prompt template not found", "NotFound", 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate prompt template key");
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            await _promptTemplateService.DeleteAsync(id, cancellationToken);
            return this.ApiSuccess<object>("Prompt template deleted", 204);
        }

        [HttpPost("{id:guid}/clone")]
        public async Task<IActionResult> Clone(Guid id, [FromBody] ClonePromptTemplateRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var clone = await _promptTemplateService.CloneAsync(id, request, CurrentUserId, cancellationToken);
                return this.ApiCreated(nameof(Get), new { id = clone.Id }, clone, "Prompt template cloned");
            }
            catch (KeyNotFoundException)
            {
                return this.ApiError("Prompt template not found", "NotFound", 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate prompt template key on clone");
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
        }

        [HttpPost("{key}/test")]
        public async Task<IActionResult> Test(string key, [FromBody] PromptTemplateTestRequest? request, CancellationToken cancellationToken = default)
        {
            try
            {
                request ??= new PromptTemplateTestRequest();
                var result = await _promptTemplateService.TestAsync(key, request, CurrentUserId, cancellationToken);
                return this.ApiSuccess(result, "Prompt template test executed");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Prompt template test failed for key {Key}", key);
                return this.ApiError(ex.Message, "ProviderError", 502);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Prompt template test failed for key {Key}", key);
                return this.ApiError("Prompt template test failed", "ProviderError", 502);
            }
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] int take = 50, CancellationToken cancellationToken = default)
        {
            var logs = await _promptTemplateService.GetRecentLogsAsync(take, cancellationToken);
            return this.ApiSuccess(logs, "Prompt logs retrieved");
        }
    }
}
