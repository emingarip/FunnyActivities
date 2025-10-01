using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for getting survey results.
    /// </summary>
    public class GetSurveyResultsQueryHandler : IRequestHandler<GetSurveyResultsQuery, SurveyResultsDto>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly ILogger<GetSurveyResultsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyResultsQueryHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="voteRepository">The vote repository.</param>
        /// <param name="logger">The logger.</param>
        public GetSurveyResultsQueryHandler(
            ISurveyRepository surveyRepository,
            IVoteRepository voteRepository,
            ILogger<GetSurveyResultsQueryHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _voteRepository = voteRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get survey results query.
        /// </summary>
        /// <param name="request">The get survey results query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The survey results DTO.</returns>
        public async Task<SurveyResultsDto> Handle(GetSurveyResultsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting survey results: {SurveyId}", request.SurveyId);

            var survey = await _surveyRepository.GetByIdAsync(request.SurveyId);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.SurveyId);
                throw new KeyNotFoundException("Survey not found");
            }

            var results = new SurveyResultsDto
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                TotalParticipants = survey.GetParticipantCount(),
                CompletedCount = survey.SurveyParticipants.Count(sp => sp.IsCompleted),
                CompletionRate = survey.GetParticipantCount() > 0
                    ? (double)survey.SurveyParticipants.Count(sp => sp.IsCompleted) / survey.GetParticipantCount() * 100
                    : 0
            };

            // Get survey with activities and votes
            var surveyWithVotes = await _surveyRepository.GetWithVotesAsync(request.SurveyId);
            if (surveyWithVotes.Any())
            {
                var surveyData = surveyWithVotes.First();

                // Process activity results
                foreach (var surveyActivity in surveyData.SurveyActivities.OrderBy(sa => sa.Order))
                {
                    var activityResult = new ActivityResultDto
                    {
                        SurveyActivityId = surveyActivity.Id,
                        ActivityId = surveyActivity.ActivityId,
                        ActivityName = surveyActivity.Activity?.Name ?? "Unknown Activity",
                        ActivityDescription = surveyActivity.Activity?.Description ?? "",
                        AverageVote = surveyActivity.GetAverageVote(),
                        VoteCount = surveyActivity.GetVoteCount()
                    };

                    // Calculate vote distribution
                    var votes = surveyActivity.SurveyVotes;
                    if (votes != null && votes.Any())
                    {
                        activityResult.VoteDistribution = votes
                            .GroupBy(v => v.VoteValue)
                            .ToDictionary(g => g.Key, g => g.Count());

                        if (request.IncludeComments)
                        {
                            activityResult.Comments = votes
                                .Where(v => !string.IsNullOrEmpty(v.Comment))
                                .Select(v => v.Comment)
                                .ToList();
                        }
                    }

                    results.ActivityResults.Add(activityResult);
                }

                // Include individual votes if requested
                if (request.IncludeIndividualVotes)
                {
                    results.Votes = surveyData.SurveyVotes
                        .Select(v => new VoteDto
                        {
                            Id = v.Id,
                            SurveyId = v.SurveyId,
                            SurveyActivityId = v.SurveyActivityId,
                            SurveyParticipantId = v.SurveyParticipantId,
                            ParticipantName = v.SurveyParticipant != null ? $"{v.SurveyParticipant.FirstName} {v.SurveyParticipant.LastName}" : null,
                            VoteValue = v.VoteValue,
                            CreatedAt = v.CreatedAt,
                            UpdatedAt = v.UpdatedAt
                        })
                        .ToList();
                }
            }

            _logger.LogInformation("Survey results retrieved successfully: {SurveyId}", request.SurveyId);

            return results;
        }
    }
}