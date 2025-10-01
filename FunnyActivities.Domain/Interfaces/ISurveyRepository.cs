using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Survey entity operations.
    /// </summary>
    public interface ISurveyRepository
    {
        /// <summary>
        /// Gets a survey by its ID.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <returns>The survey if found; otherwise, null.</returns>
        Task<Survey> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all surveys.
        /// </summary>
        /// <returns>A collection of all surveys.</returns>
        Task<IEnumerable<Survey>> GetAllAsync();

        /// <summary>
        /// Gets surveys by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of surveys created by the user.</returns>
        Task<IEnumerable<Survey>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Gets active surveys.
        /// </summary>
        /// <returns>A collection of active surveys.</returns>
        Task<IEnumerable<Survey>> GetActiveAsync();

        /// <summary>
        /// Gets surveys by title.
        /// </summary>
        /// <param name="title">The survey title.</param>
        /// <returns>A collection of surveys with the specified title.</returns>
        Task<IEnumerable<Survey>> GetByTitleAsync(string title);

        /// <summary>
        /// Adds a new survey.
        /// </summary>
        /// <param name="survey">The survey to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(Survey survey);

        /// <summary>
        /// Updates an existing survey.
        /// </summary>
        /// <param name="survey">The survey to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(Survey survey);

        /// <summary>
        /// Deletes a survey.
        /// </summary>
        /// <param name="survey">The survey to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(Survey survey);

        /// <summary>
        /// Checks if a survey exists by its ID.
        /// </summary>
        /// <param name="id">The survey ID.</param>
        /// <returns>True if the survey exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Checks if a survey exists by title.
        /// </summary>
        /// <param name="title">The survey title.</param>
        /// <returns>True if the survey exists; otherwise, false.</returns>
        Task<bool> ExistsByTitleAsync(string title);

        /// <summary>
        /// Gets surveys with their activities included.
        /// </summary>
        /// <param name="surveyId">The survey ID (optional).</param>
        /// <returns>A collection of surveys with activities.</returns>
        Task<IEnumerable<Survey>> GetWithActivitiesAsync(Guid? surveyId = null);

        /// <summary>
        /// Gets surveys with their participants included.
        /// </summary>
        /// <param name="surveyId">The survey ID (optional).</param>
        /// <returns>A collection of surveys with participants.</returns>
        Task<IEnumerable<Survey>> GetWithParticipantsAsync(Guid? surveyId = null);

        /// <summary>
        /// Gets surveys with their votes included.
        /// </summary>
        /// <param name="surveyId">The survey ID (optional).</param>
        /// <returns>A collection of surveys with votes.</returns>
        Task<IEnumerable<Survey>> GetWithVotesAsync(Guid? surveyId = null);

        /// <summary>
        /// Gets a survey by its share token.
        /// </summary>
        /// <param name="shareToken">The share token.</param>
        /// <returns>The survey if found; otherwise, null.</returns>
        Task<Survey> GetByShareTokenAsync(string shareToken);

        /// <summary>
        /// Checks if a survey activity exists by its ID.
        /// </summary>
        /// <param name="activityId">The survey activity ID.</param>
        /// <returns>True if the survey activity exists; otherwise, false.</returns>
        Task<bool> SurveyActivityExistsAsync(Guid activityId);

        /// <summary>
        /// Gets a survey activity by its ID.
        /// </summary>
        /// <param name="activityId">The survey activity ID.</param>
        /// <returns>The survey activity if found; otherwise, null.</returns>
        Task<SurveyActivity> GetSurveyActivityByIdAsync(Guid activityId);

        /// <summary>
        /// Adds a new survey participant.
        /// </summary>
        /// <param name="participant">The participant to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddParticipantAsync(SurveyParticipant participant);
    }
}