using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Services
{
    /// <summary>
    /// Interface for survey service operations.
    /// </summary>
    public interface ISurveyService
    {
        /// <summary>
        /// Creates a new survey.
        /// </summary>
        /// <param name="request">The create survey request.</param>
        /// <param name="createdByUserId">The ID of the user creating the survey.</param>
        /// <returns>The created survey DTO.</returns>
        Task<SurveyDto> CreateSurveyAsync(CreateSurveyRequest request, Guid createdByUserId);

        /// <summary>
        /// Updates an existing survey.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="request">The update survey request.</param>
        /// <param name="updatedByUserId">The ID of the user updating the survey.</param>
        /// <returns>The updated survey DTO.</returns>
        Task<SurveyDto> UpdateSurveyAsync(Guid id, UpdateSurveyRequest request, Guid updatedByUserId);

        /// <summary>
        /// Deletes a survey.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user requesting the deletion.</param>
        /// <returns>True if the survey was deleted successfully.</returns>
        Task<bool> DeleteSurveyAsync(Guid id, Guid requestedByUserId);

        /// <summary>
        /// Gets a survey by ID.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeActivities">Whether to include activities.</param>
        /// <param name="includeParticipants">Whether to include participants.</param>
        /// <param name="includeVotes">Whether to include votes.</param>
        /// <returns>The survey DTO.</returns>
        Task<SurveyDto> GetSurveyAsync(Guid id, Guid? requestedByUserId = null, bool includeActivities = true, bool includeParticipants = false, bool includeVotes = false);

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
        Task<SurveyListResult> GetSurveysAsync(
            Guid? userId = null,
            bool? isActive = null,
            bool? isCurrentlyActive = null,
            string searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "CreatedAt",
            bool sortDescending = true);

        /// <summary>
        /// Gets survey results.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeIndividualVotes">Whether to include individual votes.</param>
        /// <param name="includeComments">Whether to include comments.</param>
        /// <returns>The survey results DTO.</returns>
        Task<SurveyResultsDto> GetSurveyResultsAsync(Guid surveyId, Guid requestedByUserId, bool includeIndividualVotes = false, bool includeComments = true);

        /// <summary>
        /// Gets survey statistics.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <param name="includeDailyStatistics">Whether to include daily statistics.</param>
        /// <param name="includeDetailedActivityStats">Whether to include detailed activity statistics.</param>
        /// <returns>The survey statistics DTO.</returns>
        Task<SurveyStatisticsDto> GetSurveyStatisticsAsync(
            Guid surveyId,
            Guid requestedByUserId,
            bool includeDailyStatistics = false,
            bool includeDetailedActivityStats = true);

        /// <summary>
        /// Activates a survey.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <returns>The updated survey DTO.</returns>
        Task<SurveyDto> ActivateSurveyAsync(Guid id, Guid requestedByUserId);

        /// <summary>
        /// Deactivates a survey.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <param name="requestedByUserId">The ID of the user making the request.</param>
        /// <returns>The updated survey DTO.</returns>
        Task<SurveyDto> DeactivateSurveyAsync(Guid id, Guid requestedByUserId);

        /// <summary>
        /// Checks if a survey title already exists.
        /// </summary>
        /// <param name="title">The survey title.</param>
        /// <param name="excludeSurveyId">Survey ID to exclude from check (for updates).</param>
        /// <returns>True if the title exists.</returns>
        Task<bool> SurveyTitleExistsAsync(string title, Guid? excludeSurveyId = null);

        /// <summary>
        /// Generates a shareable URL for a survey.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <param name="baseUrl">The base URL of the application.</param>
        /// <returns>The shareable URL.</returns>
        Task<string> GenerateShareUrlAsync(Guid surveyId, string baseUrl);
    }
}