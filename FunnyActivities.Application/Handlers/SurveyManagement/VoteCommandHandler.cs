using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for voting on survey activities.
    /// </summary>
    public class VoteCommandHandler : IRequestHandler<VoteCommand, VoteDto>
    {
        private readonly IVoteRepository _voteRepository;
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<VoteCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoteCommandHandler"/> class.
        /// </summary>
        /// <param name="voteRepository">The vote repository.</param>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        public VoteCommandHandler(
            IVoteRepository voteRepository,
            ISurveyRepository surveyRepository,
            ILogger<VoteCommandHandler> logger)
        {
            _voteRepository = voteRepository;
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the vote command.
        /// </summary>
        /// <param name="request">The vote command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created vote DTO.</returns>
        public async Task<VoteDto> Handle(VoteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("VoteCommandHandler.Handle called. VoteCommand: SurveyActivityId={SurveyActivityId}, VoteValue={VoteValue}, SurveyParticipantId={SurveyParticipantId}",
                request.SurveyActivityId, request.VoteValue, request.SurveyParticipantId);

            // Check if participant has already voted for this activity
            var existingVote = await _voteRepository.GetByParticipantAndActivityAsync(request.SurveyParticipantId, request.SurveyActivityId);
            if (existingVote != null)
            {
                _logger.LogWarning("Participant {SurveyParticipantId} has already voted for activity {SurveyActivityId}", request.SurveyParticipantId, request.SurveyActivityId);
                throw new InvalidOperationException("Participant has already voted for this activity");
            }

            // Get the survey activity to validate it exists and get survey info
            _logger.LogInformation("Getting survey activity with ID: {SurveyActivityId}", request.SurveyActivityId);
            var surveyActivity = await _surveyRepository.GetSurveyActivityByIdAsync(request.SurveyActivityId);
            if (surveyActivity == null)
            {
                _logger.LogWarning("Survey activity not found: {SurveyActivityId}", request.SurveyActivityId);
                throw new ArgumentException("Survey activity not found");
            }

            // Validate vote value
            _logger.LogInformation("Validating vote value: {VoteValue}", request.VoteValue);
            if (request.VoteValue < 1 || request.VoteValue > 5)
            {
                _logger.LogWarning("Invalid vote value: {VoteValue}. Must be between 1 and 5", request.VoteValue);
                throw new ArgumentException("Invalid vote value");
            }

            // Create the vote
            var vote = new SurveyVote(
                surveyActivity.SurveyId,
                request.SurveyActivityId,
                request.SurveyParticipantId,
                request.VoteValue,
                null
            );

            await _voteRepository.AddAsync(vote);

            _logger.LogInformation("Vote created successfully with ID: {VoteId}", vote.Id);

            // Check if participant has completed all activities
            await CheckAndUpdateParticipantCompletionAsync(request.SurveyParticipantId, surveyActivity.SurveyId);

            return MapToDto(vote);
        }

        private async Task CheckAndUpdateParticipantCompletionAsync(Guid surveyParticipantId, Guid surveyId)
        {
            _logger.LogInformation("Checking completion status for participant {SurveyParticipantId} in survey {SurveyId}",
                surveyParticipantId, surveyId);

            // Get the survey with activities, votes, and participants
            var surveyWithVotes = await _surveyRepository.GetWithVotesAsync(surveyId);
            if (!surveyWithVotes.Any())
            {
                _logger.LogWarning("Survey not found: {SurveyId}", surveyId);
                return;
            }

            var survey = surveyWithVotes.First();

            // Ensure participants are loaded
            if (!survey.SurveyParticipants.Any())
            {
                // Load participants separately if not included
                var surveyWithParticipants = await _surveyRepository.GetWithParticipantsAsync(surveyId);
                if (surveyWithParticipants.Any())
                {
                    survey.SurveyParticipants = surveyWithParticipants.First().SurveyParticipants.ToList();
                }
            }

            var participant = survey.SurveyParticipants.FirstOrDefault(p => p.Id == surveyParticipantId);
            if (participant == null)
            {
                _logger.LogWarning("Participant not found: {SurveyParticipantId}", surveyParticipantId);
                return;
            }

            // Get all survey activity IDs
            var surveyActivityIds = survey.SurveyActivities.Select(sa => sa.Id).ToList();

            // Get participant's votes
            var participantVotes = survey.SurveyVotes.Where(v => v.SurveyParticipantId == surveyParticipantId).ToList();
            var votedActivityIds = participantVotes.Select(v => v.SurveyActivityId).ToList();

            // Check if participant has voted on all activities
            var hasVotedOnAllActivities = surveyActivityIds.All(activityId => votedActivityIds.Contains(activityId));

            _logger.LogInformation("Participant {SurveyParticipantId} completion check: Has voted on {VotedCount}/{TotalCount} activities",
                surveyParticipantId, votedActivityIds.Count, surveyActivityIds.Count);

            if (hasVotedOnAllActivities && !participant.IsCompleted)
            {
                participant.MarkAsCompleted();
                await _surveyRepository.UpdateAsync(survey);
                _logger.LogInformation("Participant {SurveyParticipantId} marked as completed and saved", surveyParticipantId);
            }
            else if (!hasVotedOnAllActivities && participant.IsCompleted)
            {
                participant.MarkAsIncomplete();
                await _surveyRepository.UpdateAsync(survey);
                _logger.LogInformation("Participant {SurveyParticipantId} marked as incomplete and saved", surveyParticipantId);
            }
        }

        private VoteDto MapToDto(SurveyVote vote)
        {
            return new VoteDto
            {
                Id = vote.Id,
                SurveyId = vote.SurveyId,
                SurveyActivityId = vote.SurveyActivityId,
                SurveyParticipantId = vote.SurveyParticipantId,
                ParticipantName = vote.SurveyParticipant != null ? $"{vote.SurveyParticipant.FirstName} {vote.SurveyParticipant.LastName}" : null,
                VoteValue = vote.VoteValue,
                CreatedAt = vote.CreatedAt,
                UpdatedAt = vote.UpdatedAt
            };
        }
    }
}