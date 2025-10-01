using System;
using System.Threading.Tasks;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Services
{
    /// <summary>
    /// Interface for voting service operations.
    /// </summary>
    public interface IVotingService
    {
        /// <summary>
        /// Casts a vote for a survey activity.
        /// </summary>
        /// <param name="request">The vote request.</param>
        /// <param name="userId">The ID of the user voting.</param>
        /// <returns>The created vote DTO.</returns>
        Task<VoteDto> VoteAsync(VoteRequest request, Guid userId);

        /// <summary>
        /// Updates an existing vote.
        /// </summary>
        /// <param name="voteId">The vote ID.</param>
        /// <param name="request">The vote request with updated information.</param>
        /// <param name="userId">The ID of the user updating the vote.</param>
        /// <returns>The updated vote DTO.</returns>
        Task<VoteDto> UpdateVoteAsync(Guid voteId, VoteRequest request, Guid userId);

        /// <summary>
        /// Deletes a vote.
        /// </summary>
        /// <param name="voteId">The vote ID.</param>
        /// <param name="userId">The ID of the user deleting the vote.</param>
        /// <returns>True if the vote was deleted successfully.</returns>
        Task<bool> DeleteVoteAsync(Guid voteId, Guid userId);

        /// <summary>
        /// Gets a vote by ID.
        /// </summary>
        /// <param name="voteId">The vote ID.</param>
        /// <param name="userId">The ID of the user requesting the vote.</param>
        /// <returns>The vote DTO.</returns>
        Task<VoteDto> GetVoteAsync(Guid voteId, Guid userId);

        /// <summary>
        /// Checks if a survey participant has already voted for a survey activity.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>True if the survey participant has voted.</returns>
        Task<bool> HasParticipantVotedAsync(Guid surveyParticipantId, Guid surveyActivityId);

        /// <summary>
        /// Gets all votes by a survey participant.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <returns>A list of vote DTOs.</returns>
        Task<System.Collections.Generic.List<VoteDto>> GetParticipantVotesAsync(Guid surveyParticipantId);

        /// <summary>
        /// Gets all votes by a user for a specific survey.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>A list of vote DTOs.</returns>
        Task<System.Collections.Generic.List<VoteDto>> GetUserVotesForSurveyAsync(Guid userId, Guid surveyId);

        /// <summary>
        /// Gets the average vote for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The average vote value.</returns>
        Task<double> GetAverageVoteForActivityAsync(Guid surveyActivityId);

        /// <summary>
        /// Gets the vote count for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The vote count.</returns>
        Task<int> GetVoteCountForActivityAsync(Guid surveyActivityId);

        /// <summary>
        /// Validates if a vote value is within the allowed range (1-5).
        /// </summary>
        /// <param name="voteValue">The vote value to validate.</param>
        /// <returns>True if the vote value is valid.</returns>
        bool IsValidVoteValue(int voteValue);

        /// <summary>
        /// Checks if a survey is currently active and accepting votes.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>True if the survey is accepting votes.</returns>
        Task<bool> IsSurveyAcceptingVotesAsync(Guid surveyId);
    }
}