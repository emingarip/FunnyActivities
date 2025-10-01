using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for getting a public survey by share token.
    /// </summary>
    public class GetSurveyByShareTokenQueryHandler : IRequestHandler<GetSurveyByShareTokenQuery, SurveyDto>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<GetSurveyByShareTokenQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyByShareTokenQueryHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        public GetSurveyByShareTokenQueryHandler(
            ISurveyRepository surveyRepository,
            ILogger<GetSurveyByShareTokenQueryHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get survey by share token query.
        /// </summary>
        /// <param name="request">The get survey by share token query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The survey DTO.</returns>
        public async Task<SurveyDto> Handle(GetSurveyByShareTokenQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting public survey by share token: {ShareToken}", request.ShareToken);

            var survey = await _surveyRepository.GetByShareTokenAsync(request.ShareToken);

            if (survey == null)
            {
                _logger.LogWarning("Survey not found for share token: {ShareToken}", request.ShareToken);
                throw new KeyNotFoundException("Survey not found");
            }

            _logger.LogInformation("Found survey: ID={Id}, Title={Title}, IsActive={IsActive}",
                survey.Id, survey.Title, survey.IsActive);

            // Check if survey is active and currently available
            if (!survey.IsActive || !survey.IsCurrentlyActive())
            {
                _logger.LogWarning("Survey is not active or not currently available for share token: {ShareToken}",
                    request.ShareToken);
                throw new InvalidOperationException("Survey is not available");
            }

            var surveyDto = MapToDto(survey);

            // Load activities if requested
            if (request.IncludeActivities)
            {
                var surveyWithActivities = await _surveyRepository.GetWithActivitiesAsync(survey.Id);
                if (surveyWithActivities.Any())
                {
                    var surveyWithActivitiesData = surveyWithActivities.First();
                    surveyDto.Activities = surveyWithActivitiesData.SurveyActivities
                        .OrderBy(sa => sa.Order)
                        .Select(sa => new SurveyActivityDto
                        {
                            Id = sa.Id,
                            SurveyId = sa.SurveyId,
                            ActivityId = sa.ActivityId,
                            ActivityName = sa.Activity?.Name ?? "Unknown Activity",
                            ActivityDescription = sa.Activity?.Description ?? "",
                            DurationMinutes = sa.Activity?.Duration != null ? (int?)sa.Activity.Duration.Value.TotalMinutes : null,
                            Order = sa.Order,
                            AverageVote = sa.GetAverageVote(),
                            VoteCount = sa.GetVoteCount(),
                            VideoUrl = sa.Activity?.VideoUrl?.Value
                        })
                        .ToList();
                }
            }

            _logger.LogInformation("Public survey retrieved successfully by share token: {ShareToken}", request.ShareToken);

            return surveyDto;
        }

        private SurveyDto MapToDto(Survey survey)
        {
            return new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                CreatedByUserId = survey.CreatedByUserId,
                IsActive = survey.IsActive,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                MaxParticipants = survey.MaxParticipants,
                ParticipantCount = survey.GetParticipantCount(),
                CreatedAt = survey.CreatedAt,
                UpdatedAt = survey.UpdatedAt,
                IsCurrentlyActive = survey.IsCurrentlyActive(),
                HasReachedMaxParticipants = survey.HasReachedMaxParticipants(),
                ShareToken = survey.ShareToken
            };
        }
    }
}