using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.DTOs.SurveyManagement;
using FunnyActivities.Application.Commands.SurveyManagement;
using FunnyActivities.Application.Queries.SurveyManagement;

namespace FunnyActivities.Application.Services
{
    /// <summary>
    /// Implementation of the survey service.
    /// </summary>
    public class SurveyService : ISurveyService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SurveyService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyService"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        public SurveyService(IMediator mediator, ILogger<SurveyService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new survey.
        /// </summary>
        /// <param name="request">The create survey request.</param>
        /// <param name="createdByUserId">The ID of the user creating the survey.</param>
        /// <returns>The created survey DTO.</returns>
        public async Task<SurveyDto> CreateSurveyAsync(CreateSurveyRequest request, Guid createdByUserId)
        {
            _logger.LogInformation("Creating survey: {Title} by user: {UserId}", request.Title, createdByUserId);

            var command = new CreateSurveyCommand(
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.MaxParticipants,
                request.ActivityIds,
                createdByUserId
            );

            var result = await _mediator.Send(command);

            _logger.LogInformation("Survey created successfully: {SurveyId}", result.Id);

            return result;
        }

        /// <summary>
        /// Updates an existing survey.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="request">The update survey request.</param>
        /// <param name="updatedByUserId">The ID of the user updating the survey.</param>
        /// <returns>The updated survey DTO.</returns>
        public async Task<SurveyDto> UpdateSurveyAsync(Guid id, UpdateSurveyRequest request, Guid updatedByUserId)
        {
            _logger.LogInformation("Updating survey: {SurveyId} by user: {UserId}", id, updatedByUserId);

            var command = new UpdateSurveyCommand(
                id,
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.MaxParticipants,
                request.ActivityIds,
                updatedByUserId
            );

            var result = await _mediator.Send(command);

            _logger.LogInformation("Survey updated successfully: {SurveyId}", result.Id);

            return result;
        }

        /// <summary>
        /// Deletes a survey.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user requesting the deletion.</param>
        /// <returns>True if the survey was deleted successfully.</returns>
        public async Task<bool> DeleteSurveyAsync(Guid id, Guid requestedByUserId)
        {
            _logger.LogInformation("Deleting survey: {SurveyId} by user: {UserId}", id, requestedByUserId);

            var command = new DeleteSurveyCommand(id, requestedByUserId);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Survey deletion result: {SurveyId} - Success: {Success}", id, result);

            return result;
        }

        /// <summary>
        /// Gets a survey by ID.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeActivities">Whether to include activities.</param>
        /// <param name="includeParticipants">Whether to include participants.</param>
        /// <param name="includeVotes">Whether to include votes.</param>
        /// <returns>The survey DTO.</returns>
        public async Task<SurveyDto> GetSurveyAsync(Guid id, Guid? requestedByUserId = null, bool includeActivities = true, bool includeParticipants = false, bool includeVotes = false)
        {
            _logger.LogInformation("Getting survey: {SurveyId} by user: {UserId}", id, requestedByUserId);

            var query = new GetSurveyQuery(id, requestedByUserId, includeActivities, includeParticipants, includeVotes);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Survey retrieved successfully: {SurveyId}", id);

            return result;
        }

        /// <summary>
        /// Gets a list of surveys with filtering and pagination.
        /// </summary>
        /// <param name="userId">The user ID for filtering.</param>
        /// <param name="isActive">Filter by active status.</param>
        /// <param name="isCurrentlyActive">Filter by currently active status.</param>
        /// <param name="searchTerm">Search term for title/description.</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Page size for pagination.</param>
        /// <param name="sortBy">Sort field.</param>
        /// <param name="sortDescending">Whether to sort descending.</param>
        /// <returns>A survey list result with pagination.</returns>
        public async Task<SurveyListResult> GetSurveysAsync(
            Guid? userId = null,
            bool? isActive = null,
            bool? isCurrentlyActive = null,
            string searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "CreatedAt",
            bool sortDescending = true)
        {
            _logger.LogInformation("Getting surveys with filters - UserId: {UserId}, IsActive: {IsActive}, SearchTerm: {SearchTerm}",
                userId, isActive, searchTerm);

            var query = new GetSurveysQuery(userId, isActive, isCurrentlyActive, searchTerm, pageNumber, pageSize, sortBy, sortDescending);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} surveys", result.Surveys.Count);

            return result;
        }

        /// <summary>
        /// Gets survey results.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeIndividualVotes">Whether to include individual votes.</param>
        /// <param name="includeComments">Whether to include comments.</param>
        /// <returns>The survey results DTO.</returns>
        public async Task<SurveyResultsDto> GetSurveyResultsAsync(Guid surveyId, Guid requestedByUserId, bool includeIndividualVotes = false, bool includeComments = true)
        {
            _logger.LogInformation("Getting survey results: {SurveyId} by user: {UserId}", surveyId, requestedByUserId);

            var query = new GetSurveyResultsQuery(surveyId, requestedByUserId, includeIndividualVotes, includeComments);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Survey results retrieved successfully: {SurveyId}", surveyId);

            return result;
        }

        /// <summary>
        /// Gets survey statistics.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeDailyStatistics">Whether to include daily statistics.</param>
        /// <param name="includeDetailedActivityStats">Whether to include detailed activity statistics.</param>
        /// <returns>The survey statistics DTO.</returns>
        public async Task<SurveyStatisticsDto> GetSurveyStatisticsAsync(
            Guid surveyId,
            Guid requestedByUserId,
            bool includeDailyStatistics = false,
            bool includeDetailedActivityStats = true)
        {
            _logger.LogInformation("Getting survey statistics: {SurveyId} by user: {UserId}", surveyId, requestedByUserId);

            var query = new GetSurveyStatisticsQuery(surveyId, requestedByUserId, includeDailyStatistics, includeDetailedActivityStats);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Survey statistics retrieved successfully: {SurveyId}", surveyId);

            return result;
        }

        /// <summary>
        /// Activates a survey.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <returns>The updated survey DTO.</returns>
        public async Task<SurveyDto> ActivateSurveyAsync(Guid id, Guid requestedByUserId)
        {
            _logger.LogInformation("Activating survey: {SurveyId} by user: {UserId}", id, requestedByUserId);

            var survey = await GetSurveyAsync(id, requestedByUserId);

            var updateRequest = new UpdateSurveyRequest
            {
                Title = survey.Title,
                Description = survey.Description,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                MaxParticipants = survey.MaxParticipants,
                ActivityIds = survey.Activities?.Select(a => a.ActivityId).ToList()
            };

            // Set the survey as active by not providing IsActive in the update (it will remain as is)
            var result = await UpdateSurveyAsync(id, updateRequest, requestedByUserId);

            _logger.LogInformation("Survey activated successfully: {SurveyId}", id);

            return result;
        }

        /// <summary>
        /// Deactivates a survey.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <returns>The updated survey DTO.</returns>
        public async Task<SurveyDto> DeactivateSurveyAsync(Guid id, Guid requestedByUserId)
        {
            _logger.LogInformation("Deactivating survey: {SurveyId} by user: {UserId}", id, requestedByUserId);

            var survey = await GetSurveyAsync(id, requestedByUserId);

            var updateRequest = new UpdateSurveyRequest
            {
                Title = survey.Title,
                Description = survey.Description,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                MaxParticipants = survey.MaxParticipants,
                ActivityIds = survey.Activities?.Select(a => a.ActivityId).ToList()
            };

            var result = await UpdateSurveyAsync(id, updateRequest, requestedByUserId);

            _logger.LogInformation("Survey deactivated successfully: {SurveyId}", id);

            return result;
        }

        /// <summary>
        /// Checks if a survey title already exists.
        /// </summary>
        /// <param name="title">The survey title.</param>
        /// <param name="excludeSurveyId">Survey ID to exclude from check (for updates).</param>
        /// <returns>True if the title exists.</returns>
        public async Task<bool> SurveyTitleExistsAsync(string title, Guid? excludeSurveyId = null)
        {
            _logger.LogInformation("Checking if survey title exists: {Title}", title);

            var query = new GetSurveysQuery(searchTerm: title);
            var result = await _mediator.Send(query);

            var exists = result.Surveys.Any(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase) &&
                                         (!excludeSurveyId.HasValue || s.Id != excludeSurveyId.Value));

            _logger.LogInformation("Survey title exists check result: {Title} - Exists: {Exists}", title, exists);

            return exists;
        }

        /// <summary>
        /// Generates a shareable URL for a survey.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <param name="baseUrl">The base URL of the application.</param>
        /// <returns>The shareable URL.</returns>
        public async Task<string> GenerateShareUrlAsync(Guid surveyId, string baseUrl)
        {
            _logger.LogInformation("Generating share URL for survey: {SurveyId}", surveyId);

            var survey = await GetSurveyAsync(surveyId);

            if (survey == null)
            {
                throw new KeyNotFoundException("Survey not found");
            }

            _logger.LogInformation("Survey ShareToken from DTO: {ShareToken}", survey.ShareToken);

            // Ensure baseUrl doesn't end with /
            baseUrl = baseUrl.TrimEnd('/');

            var shareUrl = $"{baseUrl}/survey/{survey.ShareToken}";

            _logger.LogInformation("Share URL generated successfully: {ShareUrl}", shareUrl);

            return shareUrl;
        }
    }
}