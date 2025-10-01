using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for SurveyVote entity operations.
    /// </summary>
    public interface IVoteRepository
    {
        /// <summary>
        /// Gets a vote by its ID.
        /// </summary>
        /// <param name="id">The vote ID.</param>
        /// <returns>The vote if found; otherwise, null.</returns>
        Task<SurveyVote> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all votes for a survey.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>A collection of votes for the survey.</returns>
        Task<IEnumerable<SurveyVote>> GetBySurveyIdAsync(Guid surveyId);

        /// <summary>
        /// Gets all votes for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>A collection of votes for the survey activity.</returns>
        Task<IEnumerable<SurveyVote>> GetBySurveyActivityIdAsync(Guid surveyActivityId);

        /// <summary>
        /// Gets all votes by a survey participant.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <returns>A collection of votes by the survey participant.</returns>
        Task<IEnumerable<SurveyVote>> GetByParticipantIdAsync(Guid surveyParticipantId);

        /// <summary>
        /// Gets a specific vote by participant and survey activity.
        /// </summary>
        /// <param name="participantId">The participant ID.</param>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The vote if found; otherwise, null.</returns>
        Task<SurveyVote> GetByParticipantAndActivityAsync(Guid participantId, Guid surveyActivityId);

        /// <summary>
        /// Gets all votes by survey participant for a specific survey.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>A collection of votes by the survey participant for the survey.</returns>
        Task<IEnumerable<SurveyVote>> GetByParticipantAndSurveyAsync(Guid surveyParticipantId, Guid surveyId);

        /// <summary>
        /// Adds a new vote.
        /// </summary>
        /// <param name="vote">The vote to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(SurveyVote vote);

        /// <summary>
        /// Updates an existing vote.
        /// </summary>
        /// <param name="vote">The vote to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(SurveyVote vote);

        /// <summary>
        /// Deletes a vote.
        /// </summary>
        /// <param name="vote">The vote to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(SurveyVote vote);

        /// <summary>
        /// Checks if a vote exists by its ID.
        /// </summary>
        /// <param name="id">The vote ID.</param>
        /// <returns>True if the vote exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Checks if a survey participant has already voted for a survey activity.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>True if the survey participant has voted; otherwise, false.</returns>
        Task<bool> HasParticipantVotedAsync(Guid surveyParticipantId, Guid surveyActivityId);

        /// <summary>
        /// Gets the average vote value for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The average vote value.</returns>
        Task<double> GetAverageVoteForActivityAsync(Guid surveyActivityId);

        /// <summary>
        /// Gets the total vote count for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The total vote count.</returns>
        Task<int> GetVoteCountForActivityAsync(Guid surveyActivityId);

        /// <summary>
        /// Gets vote statistics for a survey.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>A dictionary with activity IDs as keys and vote statistics as values.</returns>
        Task<Dictionary<Guid, (double Average, int Count)>> GetVoteStatisticsForSurveyAsync(Guid surveyId);
    }
}