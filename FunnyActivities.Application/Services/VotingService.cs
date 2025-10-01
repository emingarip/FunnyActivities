using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.DTOs.SurveyManagement;
using FunnyActivities.Application.Commands.SurveyManagement;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Application.Services
{
    /// <summary>
    /// Implementation of the voting service.
    /// </summary>
    public class VotingService : IVotingService
    {
        private readonly IMediator _mediator;
        private readonly IVoteRepository _voteRepository;
        private readonly ILogger<VotingService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VotingService"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="voteRepository">The vote repository.</param>
        /// <param name="logger">The logger.</param>
        public VotingService(IMediator mediator, IVoteRepository voteRepository, ILogger<VotingService> logger)
        {
            _mediator = mediator;
            _voteRepository = voteRepository;
            _logger = logger;
        }

        /// <summary>
        /// Casts a vote for a survey activity.
        /// </summary>
        /// <param name="request">The vote request.</param>
        /// <param name="userId">The ID of the user voting.</param>
        /// <returns>The created vote DTO.</returns>
        public async Task<VoteDto> VoteAsync(VoteRequest request, Guid userId)
        {
            _logger.LogInformation("Processing vote for survey activity: {SurveyActivityId} by user: {UserId}",
                request.SurveyActivityId, userId);

            var command = new VoteCommand(request.SurveyActivityId, request.VoteValue, userId);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Vote processed successfully: {VoteId}", result.Id);

            return result;
        }

        /// <summary>
        /// Updates an existing vote.
        /// </summary>
        /// <param name="voteId">The vote ID.</param>
        /// <param name="request">The vote request with updated information.</param>
        /// <param name="userId">The ID of the user updating the vote.</param>
        /// <returns>The updated vote DTO.</returns>
        public async Task<VoteDto> UpdateVoteAsync(Guid voteId, VoteRequest request, Guid userId)
        {
            _logger.LogInformation("Updating vote: {VoteId} by user: {UserId}", voteId, userId);

            // For now, we'll delete the existing vote and create a new one
            // In a real implementation, you might want to have an UpdateVoteCommand
            var deleteResult = await DeleteVoteAsync(voteId, userId);
            if (!deleteResult)
            {
                throw new InvalidOperationException("Failed to delete existing vote");
            }

            var newVote = await VoteAsync(request, userId);

            _logger.LogInformation("Vote updated successfully: {VoteId}", newVote.Id);

            return newVote;
        }

        /// <summary>
        /// Deletes a vote.
        /// </summary>
        /// <param name="voteId">The vote ID.</param>
        /// <param name="userId">The ID of the user deleting the vote.</param>
        /// <returns>True if the vote was deleted successfully.</returns>
        public async Task<bool> DeleteVoteAsync(Guid voteId, Guid userId)
        {
            _logger.LogInformation("Deleting vote: {VoteId} by user: {UserId}", voteId, userId);

            // For now, we'll need to implement this in the infrastructure layer
            // This would require a DeleteVoteCommand
            _logger.LogWarning("DeleteVoteAsync not implemented yet");

            return false;
        }

        /// <summary>
        /// Gets a vote by ID.
        /// </summary>
        /// <param name="voteId">The vote ID.</param>
        /// <param name="userId">The ID of the user requesting the vote.</param>
        /// <returns>The vote DTO.</returns>
        public async Task<VoteDto> GetVoteAsync(Guid voteId, Guid userId)
        {
            _logger.LogInformation("Getting vote: {VoteId} by user: {UserId}", voteId, userId);

            // This would require a GetVoteQuery
            _logger.LogWarning("GetVoteAsync not implemented yet");

            return null;
        }

        /// <summary>
        /// Checks if a survey participant has already voted for a survey activity.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>True if the survey participant has voted.</returns>
        public async Task<bool> HasParticipantVotedAsync(Guid surveyParticipantId, Guid surveyActivityId)
        {
            _logger.LogInformation("Checking if survey participant {SurveyParticipantId} has voted for activity {SurveyActivityId}",
                surveyParticipantId, surveyActivityId);

            return await _voteRepository.HasParticipantVotedAsync(surveyParticipantId, surveyActivityId);
        }

        /// <summary>
        /// Gets all votes by a survey participant.
        /// </summary>
        /// <param name="surveyParticipantId">The survey participant ID.</param>
        /// <returns>A list of vote DTOs.</returns>
        public async Task<List<VoteDto>> GetParticipantVotesAsync(Guid surveyParticipantId)
        {
            _logger.LogInformation("Getting votes for survey participant {SurveyParticipantId}", surveyParticipantId);

            var votes = await _voteRepository.GetByParticipantIdAsync(surveyParticipantId);
            return votes.Select(v => new VoteDto
            {
                Id = v.Id,
                SurveyId = v.SurveyId,
                SurveyActivityId = v.SurveyActivityId,
                SurveyParticipantId = v.SurveyParticipantId,
                VoteValue = v.VoteValue,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            }).ToList();
        }

        /// <summary>
        /// Gets all votes by a user for a specific survey.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>A list of vote DTOs.</returns>
        public async Task<List<VoteDto>> GetUserVotesForSurveyAsync(Guid userId, Guid surveyId)
        {
            _logger.LogInformation("Getting votes for user {UserId} and survey {SurveyId}", userId, surveyId);

            // This would require a GetUserVotesForSurveyQuery
            _logger.LogWarning("GetUserVotesForSurveyAsync not implemented yet");

            return new List<VoteDto>();
        }

        /// <summary>
        /// Gets the average vote for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The average vote value.</returns>
        public async Task<double> GetAverageVoteForActivityAsync(Guid surveyActivityId)
        {
            _logger.LogInformation("Getting average vote for activity: {SurveyActivityId}", surveyActivityId);

            // This would require a repository method
            _logger.LogWarning("GetAverageVoteForActivityAsync not implemented yet");

            return 0.0;
        }

        /// <summary>
        /// Gets the vote count for a survey activity.
        /// </summary>
        /// <param name="surveyActivityId">The survey activity ID.</param>
        /// <returns>The vote count.</returns>
        public async Task<int> GetVoteCountForActivityAsync(Guid surveyActivityId)
        {
            _logger.LogInformation("Getting vote count for activity: {SurveyActivityId}", surveyActivityId);

            // This would require a repository method
            _logger.LogWarning("GetVoteCountForActivityAsync not implemented yet");

            return 0;
        }

        /// <summary>
        /// Validates if a vote value is within the allowed range (1-5).
        /// </summary>
        /// <param name="voteValue">The vote value to validate.</param>
        /// <returns>True if the vote value is valid.</returns>
        public bool IsValidVoteValue(int voteValue)
        {
            return voteValue >= 1 && voteValue <= 5;
        }

        /// <summary>
        /// Checks if a survey is currently active and accepting votes.
        /// </summary>
        /// <param name="surveyId">The survey ID.</param>
        /// <returns>True if the survey is accepting votes.</returns>
        public async Task<bool> IsSurveyAcceptingVotesAsync(Guid surveyId)
        {
            _logger.LogInformation("Checking if survey {SurveyId} is accepting votes", surveyId);

            try
            {
                var survey = await _mediator.Send(new GetSurveyQuery(surveyId));
                return survey.IsActive && survey.IsCurrentlyActive && !survey.HasReachedMaxParticipants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if survey {SurveyId} is accepting votes", surveyId);
                return false;
            }
        }
    }
}