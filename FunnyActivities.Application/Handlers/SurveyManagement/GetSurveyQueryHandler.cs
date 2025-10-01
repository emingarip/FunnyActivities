using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.SurveyManagement;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Handlers.SurveyManagement
{
    /// <summary>
    /// Handler for getting a specific survey.
    /// </summary>
    public class GetSurveyQueryHandler : IRequestHandler<GetSurveyQuery, SurveyDto>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<GetSurveyQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveyQueryHandler"/> class.
        /// </summary>
        /// <param name="surveyRepository">The survey repository.</param>
        /// <param name="logger">The logger.</param>
        public GetSurveyQueryHandler(
            ISurveyRepository surveyRepository,
            ILogger<GetSurveyQueryHandler> logger)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get survey query.
        /// </summary>
        /// <param name="request">The get survey query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The survey DTO.</returns>
        public async Task<SurveyDto> Handle(GetSurveyQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting survey: {SurveyId}", request.Id);

            var survey = await _surveyRepository.GetByIdAsync(request.Id);
            if (survey == null)
            {
                _logger.LogWarning("Survey not found: {SurveyId}", request.Id);
                throw new KeyNotFoundException("Survey not found");
            }

            var surveyDto = MapToDto(survey);

            // Load activities if requested
            if (request.IncludeActivities)
            {
                var surveyWithActivities = await _surveyRepository.GetWithActivitiesAsync(request.Id);
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

            _logger.LogInformation("Survey retrieved successfully: {SurveyId}", request.Id);

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