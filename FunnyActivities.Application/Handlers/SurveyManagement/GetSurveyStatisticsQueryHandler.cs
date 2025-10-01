using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for getting survey statistics.
    /// </summary>
    public class GetSurveyStatisticsQueryHandler : IRequestHandler<GetSurveyStatisticsQuery, SurveyStatisticsDto>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly ILogger<GetSurveyStatisticsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyStatisticsQueryHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="voteRepository">The vote repository.</param>
        /// <param name="logger">The logger.</param>
        public GetSurveyStatisticsQueryHandler(
            ISurveyRepository surveyRepository,
            IVoteRepository voteRepository,
            ILogger<GetSurveyStatisticsQueryHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _voteRepository = voteRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get survey statistics query.
        /// </summary>
        /// <param name="request">The get survey statistics query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The survey statistics DTO.</returns>
        public async Task<SurveyStatisticsDto> Handle(GetSurveyStatisticsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting survey statistics: {SurveyId}", request.SurveyId);

            var survey = await _surveyRepository.GetByIdAsync(request.SurveyId);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.SurveyId);
                throw new KeyNotFoundException("Survey not found");
            }

            var statistics = new SurveyStatisticsDto
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                TotalParticipants = survey.GetParticipantCount(),
                CompletedCount = survey.SurveyParticipants.Count(sp => sp.IsCompleted),
                TotalVotes = survey.SurveyVotes?.Count ?? 0,
                CreatedAt = survey.CreatedAt,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                IsCurrentlyActive = survey.IsCurrentlyActive()
            };

            // Calculate completion rate
            statistics.CompletionRate = statistics.TotalParticipants > 0
                ? (double)statistics.CompletedCount / statistics.TotalParticipants * 100
                : 0;

            // Get survey with votes for detailed statistics
            var surveyWithVotes = await _surveyRepository.GetWithVotesAsync(request.SurveyId);
            if (surveyWithVotes.Any())
            {
                var surveyData = surveyWithVotes.First();

                // Calculate overall average vote
                if (surveyData.SurveyVotes != null && surveyData.SurveyVotes.Any())
                {
                    statistics.OverallAverageVote = surveyData.SurveyVotes.Average(v => v.VoteValue);
                }

                // Calculate overall vote distribution
                statistics.OverallVoteDistribution = surveyData.SurveyVotes
                    .GroupBy(v => v.VoteValue)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Process activity statistics
                foreach (var surveyActivity in surveyData.SurveyActivities.OrderBy(sa => sa.Order))
                {
                    var activityStats = new ActivityStatisticsDto
                    {
                        SurveyActivityId = surveyActivity.Id,
                        ActivityId = surveyActivity.ActivityId,
                        ActivityName = surveyActivity.Activity?.Name ?? "Unknown Activity",
                        AverageVote = surveyActivity.GetAverageVote(),
                        VoteCount = surveyActivity.GetVoteCount()
                    };

                    // Calculate vote distribution for this activity
                    if (surveyActivity.SurveyVotes != null && surveyActivity.SurveyVotes.Any())
                    {
                        activityStats.VoteDistribution = surveyActivity.SurveyVotes
                            .GroupBy(v => v.VoteValue)
                            .ToDictionary(g => g.Key, g => g.Count());

                        // Calculate standard deviation
                        var votes = surveyActivity.SurveyVotes.Select(v => v.VoteValue).ToList();
                        if (votes.Count > 1)
                        {
                            var average = votes.Average();
                            var sumOfSquaresOfDifferences = votes.Select(val => (val - average) * (val - average)).Sum();
                            activityStats.StandardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / votes.Count);
                        }

                        activityStats.MinVote = votes.Min();
                        activityStats.MaxVote = votes.Max();
                    }

                    statistics.ActivityStatistics.Add(activityStats);
                }

                // Include daily statistics if requested
                if (request.IncludeDailyStatistics)
                {
                    statistics.DailyStatistics = await CalculateDailyStatistics(surveyData);
                }
            }

            _logger.LogInformation("Survey statistics retrieved successfully: {SurveyId}", request.SurveyId);

            return statistics;
        }

        private async Task<List<DailyStatisticsDto>> CalculateDailyStatistics(Survey survey)
        {
            var dailyStats = new List<DailyStatisticsDto>();

            // Get all participants and votes
            var participants = survey.SurveyParticipants ?? new List<SurveyParticipant>();
            var votes = survey.SurveyVotes ?? new List<SurveyVote>();

            // Group by date
            var participantDates = participants
                .GroupBy(p => p.ParticipatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() });

            var completedDates = participants
                .Where(p => p.IsCompleted && p.CompletedAt.HasValue)
                .GroupBy(p => p.CompletedAt.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() });

            var voteDates = votes
                .GroupBy(v => v.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() });

            // Get date range from survey start to now
            var startDate = survey.StartDate.Date;
            var endDate = survey.EndDate?.Date ?? DateTime.UtcNow.Date;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dailyStat = new DailyStatisticsDto
                {
                    Date = date,
                    ParticipantCount = participantDates.FirstOrDefault(d => d.Date == date)?.Count ?? 0,
                    CompletedCount = completedDates.FirstOrDefault(d => d.Date == date)?.Count ?? 0,
                    VoteCount = voteDates.FirstOrDefault(d => d.Date == date)?.Count ?? 0
                };

                dailyStats.Add(dailyStat);
            }

            return dailyStats;
        }
    }
}