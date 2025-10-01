using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for getting a public survey.
    /// </summary>
    public class GetPublicSurveyQueryHandler : IRequestHandler<GetPublicSurveyQuery, SurveyDto>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<GetPublicSurveyQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPublicSurveyQueryHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        public GetPublicSurveyQueryHandler(
            ISurveyRepository surveyRepository,
            ILogger<GetPublicSurveyQueryHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get public survey query.
        /// </summary>
        /// <param name="request">The get public survey query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The survey DTO.</returns>
        public async Task<SurveyDto> Handle(GetPublicSurveyQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting public survey: {SurveyId}", request.SurveyId);

            var survey = await _surveyRepository.GetByIdAsync(request.SurveyId);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.SurveyId);
                throw new KeyNotFoundException("Survey not found");
            }

            // Check if survey is active and currently available
            if (!survey.IsActive || !survey.IsCurrentlyActive())
            {
                _logger.LogWarning("Survey is not active or not currently available: {SurveyId}", request.SurveyId);
                throw new InvalidOperationException("Survey is not available");
            }

            // TODO: Validate access token if needed
            // For now, we'll assume any active survey is publicly accessible
            // In a real implementation, you might want to check the access token

            var surveyDto = MapToDto(survey);

            // Load activities if requested
            if (request.IncludeActivities)
            {
                var surveyWithActivities = await _surveyRepository.GetWithActivitiesAsync(request.SurveyId);
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

            _logger.LogInformation("Public survey retrieved successfully: {SurveyId}", request.SurveyId);

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