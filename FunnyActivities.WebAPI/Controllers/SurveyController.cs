using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.SurveyManagement;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;
using FunnyActivities.Application.DTOs.Shared;
using FunnyActivities.WebAPI.Controllers.Base;
using FunnyActivities.Application.Services;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Survey Controller for managing surveys in the system.
    /// Provides comprehensive CRUD operations for survey management.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations and results/statistics access
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/surveys")]
    public class SurveyController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ISurveyService _surveyService;
        private readonly ILogger<SurveyController> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="surveyService">The survey service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        public SurveyController(IMediator mediator, ISurveyService surveyService, ILogger<SurveyController> logger, IConfiguration configuration)
            : base(logger)
        {
            _mediator = mediator;
            _surveyService = surveyService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves a paginated list of all surveys.
        /// </summary>
        /// <remarks>
        /// Requires Admin role authorization.
        /// Returns surveys with their basic information.
        /// </remarks>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <param name="searchTerm">Optional search term for filtering surveys by title.</param>
        /// <param name="status">Optional status filter (Active, Completed, Draft).</param>
        /// <param name="sortBy">Sort field (title, createdAt, updatedAt, startDate, endDate).</param>
        /// <param name="sortOrder">Sort order (asc, desc).</param>
        /// <returns>A paginated list of surveys.</returns>
        [HttpGet]
        [Authorize(Policy = "CanViewSurvey")]
        [ProducesResponseType(typeof(SurveyListResult), 200)]
        public async Task<IActionResult> GetSurveys(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] string? sortOrder = "desc")
        {
            _logger.LogInformation("Retrieving surveys with page: {PageNumber}, pageSize: {PageSize}, searchTerm: {SearchTerm}, status: {Status}", pageNumber, pageSize, searchTerm, status);

            // Validate pageSize
            if (pageSize > 100)
            {
                pageSize = 100;
                _logger.LogWarning("PageSize capped at 100");
            }

            var query = new GetSurveysQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortOrder == "desc"
            };

            var result = await _mediator.Send(query);
            _logger.LogInformation("Surveys query returned {Count} items, total {TotalCount}", result.Surveys.Count, result.TotalCount);

            return this.ApiSuccess(result, "Surveys retrieved successfully");
        }

        /// <summary>
        /// Retrieves a specific survey by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the survey.</param>
        /// <returns>The survey information.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewSurvey")]
        [ProducesResponseType(typeof(SurveyDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSurvey(Guid id)
        {
            _logger.LogInformation("Retrieving survey with ID: {SurveyId}", id);

            var query = new GetSurveyQuery { Id = id };
            var survey = await _mediator.Send(query);

            if (survey == null)
            {
                _logger.LogWarning("Survey with ID {SurveyId} not found", id);
                return this.ApiError("Survey not found", "NotFound", 404);
            }

            return this.ApiSuccess(survey, "Survey retrieved successfully");
        }

        /// <summary>
        /// Creates a new survey.
        /// </summary>
        /// <param name="request">The survey creation request.</param>
        /// <returns>The created survey.</returns>
        [HttpPost]
        [Authorize(Policy = "CanCreateSurvey")]
        [ProducesResponseType(typeof(SurveyDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyRequest request)
        {
            _logger.LogInformation("Creating new survey: {Title}", request.Title);

            var command = new CreateSurveyCommand
            {
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ActivityIds = request.ActivityIds,
                CreatedByUserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Survey created successfully with ID: {Id}", result.Id);
                return this.ApiCreated(nameof(GetSurvey), new { id = result.Id }, result, "Survey created successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Survey creation failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating survey");
                return this.ApiError("An error occurred while creating the survey", "InternalError", 500);
            }
        }

        /// <summary>
        /// Updates an existing survey.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to update.</param>
        /// <param name="request">The survey update request.</param>
        /// <returns>The updated survey.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanUpdateSurvey")]
        [ProducesResponseType(typeof(SurveyDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSurvey(Guid id, [FromBody] UpdateSurveyRequest request)
        {
            _logger.LogInformation("Updating survey with ID: {SurveyId}", id);

            var command = new UpdateSurveyCommand
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ActivityIds = request.ActivityIds,
                UpdatedByUserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Survey updated successfully with ID: {SurveyId}", result.Id);
                return this.ApiSuccess(result, "Survey updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Survey update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Survey update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating survey");
                return this.ApiError("An error occurred while updating the survey", "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes a survey.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDeleteSurvey")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSurvey(Guid id)
        {
            _logger.LogInformation("Deleting survey with ID: {SurveyId}", id);

            var command = new DeleteSurveyCommand
            {
                Id = id,
                RequestedByUserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Survey deleted successfully with ID: {SurveyId}", id);
                return this.ApiSuccess<object>("Survey deleted successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Survey deletion failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting survey");
                return this.ApiError("An error occurred while deleting the survey", "InternalError", 500);
            }
        }

        /// <summary>
        /// Retrieves survey results with vote statistics.
        /// </summary>
        /// <param name="id">The unique identifier of the survey.</param>
        /// <returns>The survey results with detailed statistics.</returns>
        [HttpGet("{id}/results")]
        [Authorize(Policy = "CanViewSurveyResults")]
        [ProducesResponseType(typeof(SurveyResultsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSurveyResults(Guid id)
        {
            _logger.LogInformation("Retrieving survey results for ID: {SurveyId}", id);

            var query = new GetSurveyResultsQuery { SurveyId = id };
            var results = await _mediator.Send(query);

            if (results == null)
            {
                _logger.LogWarning("Survey results not found for ID {SurveyId}", id);
                return this.ApiError("Survey not found", "NotFound", 404);
            }

            return this.ApiSuccess(results, "Survey results retrieved successfully");
        }

        /// <summary>
        /// Retrieves survey statistics summary.
        /// </summary>
        /// <param name="id">The unique identifier of the survey.</param>
        /// <returns>The survey statistics summary.</returns>
        [HttpGet("{id}/statistics")]
        [Authorize(Policy = "CanViewSurveyStatistics")]
        [ProducesResponseType(typeof(SurveyStatisticsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSurveyStatistics(Guid id)
        {
            _logger.LogInformation("Retrieving survey statistics for ID: {SurveyId}", id);

            var query = new GetSurveyStatisticsQuery { SurveyId = id };
            var statistics = await _mediator.Send(query);

            if (statistics == null)
            {
                _logger.LogWarning("Survey statistics not found for ID {SurveyId}", id);
                return this.ApiError("Survey not found", "NotFound", 404);
            }

            return this.ApiSuccess(statistics, "Survey statistics retrieved successfully");
        }

        /// <summary>
        /// Retrieves survey participants.
        /// </summary>
        /// <param name="id">The unique identifier of the survey.</param>
        /// <returns>The list of survey participants.</returns>
        [HttpGet("{id}/participants")]
        [Authorize(Policy = "CanViewSurvey")]
        [ProducesResponseType(typeof(IEnumerable<SurveyParticipantDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSurveyParticipants(Guid id)
        {
            _logger.LogInformation("Retrieving survey participants for ID: {SurveyId}", id);

            var query = new GetSurveyParticipantsQuery { SurveyId = id };
            var participants = await _mediator.Send(query);

            return this.ApiSuccess(participants, "Survey participants retrieved successfully");
        }

        /// <summary>
        /// Generates a shareable URL for a survey.
        /// </summary>
        /// <param name="id">The unique identifier of the survey.</param>
        /// <returns>The shareable URL for the survey.</returns>
        [HttpGet("{id}/share-url")]
        [Authorize(Policy = "CanViewSurvey")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetShareUrl(Guid id)
        {
            _logger.LogInformation("Generating share URL for survey ID: {SurveyId}", id);

            try
            {
                // Get the frontend base URL from configuration
                var baseUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
                _logger.LogInformation("Frontend base URL from configuration: {BaseUrl}", baseUrl);
                var shareUrl = await _surveyService.GenerateShareUrlAsync(id, baseUrl);

                var result = new
                {
                    SurveyId = id,
                    ShareUrl = shareUrl,
                    ShareToken = "" // We'll get this from the survey if needed
                };

                // Get the survey to include the share token
                var survey = await _surveyService.GetSurveyAsync(id, CurrentUserId);
                if (survey != null)
                {
                    result = new
                    {
                        SurveyId = id,
                        ShareUrl = shareUrl,
                        ShareToken = survey.ShareToken
                    };
                }

                return this.ApiSuccess(result, "Share URL generated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Share URL generation failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating share URL for survey {SurveyId}", id);
                return this.ApiError("An error occurred while generating the share URL", "InternalError", 500);
            }
        }
    }
}